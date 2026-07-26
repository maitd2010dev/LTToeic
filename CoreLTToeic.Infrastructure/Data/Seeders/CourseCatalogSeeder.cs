using System.Text.Json;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Domain.Enums;
using CoreLTToeic.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLTToeic.Infrastructure.Data.Seeders;

public class CourseCatalogSeeder
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly ILogger<CourseCatalogSeeder> _logger;

    public CourseCatalogSeeder(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger<CourseCatalogSeeder> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task SeedAsync(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            _logger.LogWarning("Course catalog seed data file not found: {path}", jsonFilePath);
            return;
        }

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var data = JsonSerializer.Deserialize<CourseCatalogSeedJson>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        ValidateConfig(data, jsonFilePath);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var sourceCourse = await context.Courses
            .AsNoTracking()
            .AsSplitQuery()
            .Include(course => course.Sections)
                .ThenInclude(section => section.Lessons)
                    .ThenInclude(lesson => lesson.QuizQuestions)
                        .ThenInclude(question => question.Option)
            .FirstOrDefaultAsync(course => course.Title == data!.SourceCourseTitle);

        if (sourceCourse == null)
        {
            throw new InvalidDataException(
                $"Cannot seed course catalog because source course was not found: {data!.SourceCourseTitle}.");
        }

        ValidateSourceCourse(sourceCourse, data!);

        var requestedTitles = data!.Courses.Select(course => course.Title).ToList();
        var existingCourses = await context.Courses
            .Where(course => requestedTitles.Contains(course.Title))
            .Select(course => new
            {
                course.Title,
                course.Level,
                course.Status,
                SectionCount = course.Sections.Count,
                LessonCount = course.Sections.SelectMany(section => section.Lessons).Count(),
                EnrollmentCount = course.Enrollments.Count
            })
            .ToListAsync();

        var itemsByTitle = data.Courses.ToDictionary(
            course => course.Title,
            StringComparer.OrdinalIgnoreCase);
        foreach (var existing in existingCourses)
        {
            var expected = itemsByTitle[existing.Title];
            var expectedLevel = Enum.Parse<CourseLevel>(expected.Level, true);
            var expectedStatus = Enum.Parse<CourseStatus>(expected.Status, true);
            if (existing.SectionCount == 0 ||
                existing.LessonCount == 0 ||
                existing.Level != expectedLevel ||
                existing.Status != expectedStatus)
            {
                throw new InvalidDataException(
                    $"Refusing to replace existing course {existing.Title}: found " +
                    $"{existing.SectionCount} sections, {existing.LessonCount} lessons, " +
                    $"and {existing.EnrollmentCount} enrollments.");
            }
        }

        var existingTitles = existingCourses
            .Select(course => course.Title)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var coursesToCreate = data.Courses
            .Where(course => !existingTitles.Contains(course.Title))
            .ToList();

        if (coursesToCreate.Count == 0)
        {
            _logger.LogInformation(
                "All {count} configured course catalog items already exist, skipping.",
                data.Courses.Count);
            return;
        }

        await using var transaction = await context.Database.BeginTransactionAsync();
        foreach (var item in coursesToCreate)
            context.Courses.Add(CloneCourse(sourceCourse, item));

        await context.SaveChangesAsync();

        var storedCourses = await context.Courses
            .Where(course => requestedTitles.Contains(course.Title))
            .Select(course => new
            {
                course.Title,
                SectionCount = course.Sections.Count,
                LessonCount = course.Sections.SelectMany(section => section.Lessons).Count(),
                QuizCount = course.Sections
                    .SelectMany(section => section.Lessons)
                    .SelectMany(lesson => lesson.QuizQuestions)
                    .Count(),
                EnrollmentCount = course.Enrollments.Count
            })
            .ToListAsync();

        if (storedCourses.Count != data.Courses.Count ||
            storedCourses.Any(course =>
                course.SectionCount == 0 ||
                course.LessonCount == 0 ||
                course.EnrollmentCount != 0))
        {
            throw new InvalidDataException(
                "Course catalog seed completed with invalid course, section, lesson, or enrollment data.");
        }

        await transaction.CommitAsync();
        _logger.LogInformation(
            "Created {created} courses from {source}; {total} configured courses now exist.",
            coursesToCreate.Count,
            sourceCourse.Title,
            storedCourses.Count);
    }

    private static Course CloneCourse(
        Course sourceCourse,
        CourseCatalogItemJson item)
    {
        var now = DateTime.UtcNow;
        var course = new Course
        {
            Title = item.Title,
            Description = item.Description,
            Objective = item.Objective,
            ThumbnailUrl = string.IsNullOrWhiteSpace(item.ThumbnailUrl)
                ? sourceCourse.ThumbnailUrl
                : item.ThumbnailUrl,
            PreviewVideoUrl = string.IsNullOrWhiteSpace(item.PreviewVideoUrl)
                ? sourceCourse.PreviewVideoUrl
                : item.PreviewVideoUrl,
            Price = item.Price,
            Level = Enum.Parse<CourseLevel>(item.Level, true),
            Status = Enum.Parse<CourseStatus>(item.Status, true),
            CreatedAt = now,
            UpdatedAt = now
        };

        var sourceSections = sourceCourse.Sections.ToDictionary(
            section => section.Title,
            StringComparer.OrdinalIgnoreCase);
        var sectionOrder = 1;
        foreach (var sectionTitle in item.SectionTitles)
        {
            var sourceSection = sourceSections[sectionTitle];
            var section = new CourseSection
            {
                Title = sourceSection.Title,
                OrderIndex = sectionOrder++,
                Course = course,
                CreatedAt = now,
                UpdatedAt = now
            };

            var lessonOrder = 1;
            foreach (var sourceLesson in sourceSection.Lessons.OrderBy(lesson => lesson.OrderIndex))
            {
                var lesson = new CourseLesson
                {
                    Title = sourceLesson.Title,
                    Description = sourceLesson.Description,
                    Type = sourceLesson.Type,
                    Duration = sourceLesson.Duration,
                    OrderIndex = lessonOrder++,
                    IsFree = sourceLesson.IsFree,
                    Content = sourceLesson.Content,
                    VideoUrl = sourceLesson.VideoUrl,
                    Section = section,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                var questionOrder = 1;
                foreach (var sourceQuestion in sourceLesson.QuizQuestions
                             .OrderBy(question => question.OrderIndex))
                {
                    var question = new QuizQuestion
                    {
                        Question = sourceQuestion.Question,
                        Type = sourceQuestion.Type,
                        OrderIndex = questionOrder++,
                        Lesson = lesson,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    if (sourceQuestion.Option != null)
                    {
                        question.Option = new QuizQuestionOption
                        {
                            OptionText1 = sourceQuestion.Option.OptionText1,
                            OptionText2 = sourceQuestion.Option.OptionText2,
                            OptionText3 = sourceQuestion.Option.OptionText3,
                            OptionText4 = sourceQuestion.Option.OptionText4,
                            CorrectOption = sourceQuestion.Option.CorrectOption,
                            QuizQuestion = question,
                            CreatedAt = now,
                            UpdatedAt = now
                        };
                    }

                    lesson.QuizQuestions.Add(question);
                }

                section.Lessons.Add(lesson);
            }

            course.Sections.Add(section);
        }

        return course;
    }

    private static void ValidateConfig(
        CourseCatalogSeedJson? data,
        string jsonFilePath)
    {
        if (data == null || data.SchemaVersion != 1)
        {
            throw new InvalidDataException(
                $"Course catalog seed is empty or has an unsupported schema: {jsonFilePath}.");
        }

        if (string.IsNullOrWhiteSpace(data.SourceCourseTitle))
            throw new InvalidDataException("Course catalog sourceCourseTitle is required.");

        if (data.Courses.Count == 0)
            throw new InvalidDataException("Course catalog seed must contain at least one course.");

        if (data.Courses
                .GroupBy(course => course.Title, StringComparer.OrdinalIgnoreCase)
                .Any(group => group.Count() > 1))
        {
            throw new InvalidDataException("Course catalog titles must be unique.");
        }

        foreach (var course in data.Courses)
        {
            if (string.IsNullOrWhiteSpace(course.Title) ||
                string.IsNullOrWhiteSpace(course.Description) ||
                string.IsNullOrWhiteSpace(course.Objective) ||
                course.Price < 0 ||
                course.SectionTitles.Count == 0)
            {
                throw new InvalidDataException(
                    "Every course must contain title, description, objective, non-negative price, and sections.");
            }

            if (!Enum.TryParse<CourseLevel>(course.Level, true, out _) ||
                !Enum.TryParse<CourseStatus>(course.Status, true, out _))
            {
                throw new InvalidDataException(
                    $"Course {course.Title} has an invalid level or status.");
            }

            if (course.SectionTitles
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count() != course.SectionTitles.Count)
            {
                throw new InvalidDataException(
                    $"Course {course.Title} contains duplicate section titles.");
            }
        }
    }

    private static void ValidateSourceCourse(
        Course sourceCourse,
        CourseCatalogSeedJson data)
    {
        if (sourceCourse.Sections.Count == 0 ||
            sourceCourse.Sections.SelectMany(section => section.Lessons).Count() == 0)
        {
            throw new InvalidDataException(
                "Source course must contain at least one section and lesson.");
        }

        var sourceSectionTitles = sourceCourse.Sections
            .Select(section => section.Title)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingSections = data.Courses
            .SelectMany(course => course.SectionTitles)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(title => !sourceSectionTitles.Contains(title))
            .ToList();
        if (missingSections.Count > 0)
        {
            throw new InvalidDataException(
                $"Source course is missing configured sections: {string.Join(", ", missingSections)}.");
        }

        var invalidQuiz = sourceCourse.Sections
            .SelectMany(section => section.Lessons)
            .SelectMany(lesson => lesson.QuizQuestions)
            .Any(question =>
                question.Option == null ||
                string.IsNullOrWhiteSpace(question.Option.OptionText1) ||
                string.IsNullOrWhiteSpace(question.Option.OptionText2) ||
                string.IsNullOrWhiteSpace(question.Option.OptionText3) ||
                question.Option.CorrectOption is not ("1" or "2" or "3" or "4"));
        if (invalidQuiz)
            throw new InvalidDataException("Every source quiz question must have valid options and answer.");
    }
}
