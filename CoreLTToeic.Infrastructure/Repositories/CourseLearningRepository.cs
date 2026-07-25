using System.Data;
using CoreLTToeic.Application.Interfaces.IRepository;
using CoreLTToeic.Application.Models.EditModels;
using CoreLTToeic.Application.Models.ViewModels;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Domain.Enums;
using CoreLTToeic.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace CoreLTToeic.Infrastructure.Repositories
{
    public class CourseLearningRepository : ICourseLearningRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;

        public CourseLearningRepository(IDbContextFactory<AppDbContext> factory)
        {
            _factory = factory;
        }

        public async Task<List<CourseCatalogItemViewModel>> GetPublishedCoursesAsync(string? userId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var courses = await PublishedGraph(ctx).AsNoTracking().OrderByDescending(c => c.CreatedAt).ToListAsync();
            return courses.Select(course => MapCatalog(course, userId)).ToList();
        }

        public async Task<CourseDetailViewModel?> GetCourseDetailsAsync(long courseId, string? userId, bool includeLearningContent)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var course = await PublishedGraph(ctx).AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) return null;

            var model = new CourseDetailViewModel();
            CopyCatalog(MapCatalog(course, userId), model);
            model.Objective = course.Objective;
            model.PreviewVideoUrl = course.PreviewVideoUrl;
            model.Reviews = course.Reviews.OrderByDescending(r => r.CreatedAt).Select(review => new CourseReviewViewModel
            {
                Id = review.Id,
                UserId = review.UserId,
                UserDisplayName = review.User.FullName ?? review.User.UserName ?? "Học viên",
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                IsCurrentUser = userId != null && review.UserId == userId
            }).ToList();

            var enrollment = course.Enrollments.FirstOrDefault(e => e.UserId == userId && e.Status != EnrollmentStatus.Cancelled);
            var enrolled = enrollment != null;
            var completedIds = userId == null
                ? new HashSet<long>()
                : (await ctx.LessonCompletions.AsNoTracking()
                    .Where(c => c.UserId == userId && c.LessonId != null && c.Lesson!.Section.CourseId == courseId)
                    .Select(c => c.LessonId!.Value).ToListAsync()).ToHashSet();

            model.Sections = course.Sections.OrderBy(s => s.OrderIndex).ThenBy(s => s.Id).Select(section =>
            {
                var sectionModel = new CourseSectionLearningViewModel
                {
                    Id = section.Id,
                    Title = section.Title,
                    OrderIndex = section.OrderIndex ?? 0,
                    CompletedLessons = section.Lessons.Count(l => completedIds.Contains(l.Id))
                };
                sectionModel.Lessons = section.Lessons.OrderBy(l => l.OrderIndex).ThenBy(l => l.Id).Select(lesson =>
                {
                    var canAccess = userId != null && enrolled;
                    var lessonModel = new CourseLessonLearningViewModel
                    {
                        Id = lesson.Id,
                        SectionId = section.Id,
                        Title = lesson.Title,
                        Description = lesson.Description,
                        Type = lesson.Type ?? LessonType.Text,
                        Duration = lesson.Duration ?? 0,
                        OrderIndex = lesson.OrderIndex ?? 0,
                        IsFree = lesson.IsFree ?? false,
                        CanAccess = canAccess,
                        IsCompleted = completedIds.Contains(lesson.Id)
                    };
                    if (includeLearningContent && canAccess)
                    {
                        lessonModel.Content = lesson.Type == LessonType.Text ? lesson.Content : null;
                        lessonModel.VideoUrl = lesson.Type == LessonType.Video ? lesson.VideoUrl : null;
                        if (lesson.Type == LessonType.Quiz)
                            lessonModel.QuizQuestions = lesson.QuizQuestions.OrderBy(q => q.OrderIndex).ThenBy(q => q.Id)
                                .Select((question, index) => MapLearnerQuestion(question, index + 1)).ToList();
                    }
                    return lessonModel;
                }).ToList();
                return sectionModel;
            }).ToList();
            return model;
        }

        public async Task<List<CourseCatalogItemViewModel>> GetMyCoursesAsync(string userId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var courses = await PublishedGraph(ctx)
                .Where(c => c.Enrollments.Any(e => e.UserId == userId && e.Status != EnrollmentStatus.Cancelled))
                .AsNoTracking().OrderByDescending(c => c.CreatedAt).ToListAsync();
            return courses.Select(course => MapCatalog(course, userId)).ToList();
        }

        public async Task EnrollAsync(long courseId, string userId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            if (!await ctx.Courses.AnyAsync(c => c.Id == courseId && c.Status == CourseStatus.Published))
                throw new KeyNotFoundException("Khóa học không tồn tại hoặc chưa được xuất bản.");
            var existing = await ctx.CourseEnrollments.FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId);
            if (existing != null)
            {
                if (existing.Status == EnrollmentStatus.Cancelled)
                {
                    existing.Status = EnrollmentStatus.Active;
                    existing.EnrolledAt = DateTime.UtcNow;
                    await ctx.SaveChangesAsync();
                }
                return;
            }

            ctx.CourseEnrollments.Add(new CourseEnrollment
            {
                CourseId = courseId,
                UserId = userId,
                Status = EnrollmentStatus.Active,
                EnrolledAt = DateTime.UtcNow
            });
            try
            {
                await ctx.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ctx.ChangeTracker.Clear();
                if (!await ctx.CourseEnrollments.AnyAsync(e => e.CourseId == courseId && e.UserId == userId))
                    throw;
            }
        }

        public async Task CompleteLessonAsync(long courseId, long lessonId, string userId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var lesson = await ctx.CourseLessons.Include(l => l.Section)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.Section.CourseId == courseId)
                ?? throw new KeyNotFoundException("Không tìm thấy bài học.");
            if (lesson.Type == LessonType.Quiz)
                throw new InvalidOperationException("Hãy nộp bài kiểm tra để hoàn thành bài học này.");
            await RequireActiveEnrollmentAsync(ctx, courseId, userId);

            if (!await ctx.LessonCompletions.AnyAsync(c => c.UserId == userId && c.LessonId == lessonId))
            {
                ctx.LessonCompletions.Add(new LessonCompletion
                {
                    UserId = userId,
                    LessonId = lessonId,
                    CompletedAt = DateTime.UtcNow
                });
                try
                {
                    await ctx.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    ctx.ChangeTracker.Clear();
                    if (!await ctx.LessonCompletions.AnyAsync(c => c.UserId == userId && c.LessonId == lessonId))
                        throw;
                }
            }
            await RecalculateEnrollmentAsync(ctx, courseId, userId);
        }

        public async Task<QuizAttemptViewModel> SubmitQuizAsync(long courseId, long lessonId, string userId, QuizSubmissionEditModel model)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            await using var transaction = await ctx.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            await RequireActiveEnrollmentAsync(ctx, courseId, userId);
            var lesson = await ctx.CourseLessons.Include(l => l.Section)
                .Include(l => l.QuizQuestions).ThenInclude(q => q.Option)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.Section.CourseId == courseId && l.Type == LessonType.Quiz)
                ?? throw new KeyNotFoundException("Không tìm thấy bài kiểm tra.");
            var questions = lesson.QuizQuestions.OrderBy(q => q.OrderIndex).ThenBy(q => q.Id).ToList();
            if (questions.Count == 0)
                throw new InvalidOperationException("Bài kiểm tra chưa có câu hỏi.");
            if (model.Answers.Count != questions.Count || questions.Any(q => !model.Answers.ContainsKey(q.Id)))
                throw new InvalidOperationException("Vui lòng trả lời tất cả câu hỏi.");

            var answers = new List<QuizAttemptAnswer>();
            foreach (var question in questions)
            {
                var option = question.Option ?? throw new InvalidOperationException("Câu hỏi chưa được cấu hình đầy đủ.");
                var selected = model.Answers[question.Id];
                var validKeys = string.IsNullOrWhiteSpace(option.OptionText4)
                    ? new[] { "1", "2", "3" }
                    : new[] { "1", "2", "3", "4" };
                if (!validKeys.Contains(selected))
                    throw new InvalidOperationException("Lựa chọn gửi lên không hợp lệ.");
                answers.Add(new QuizAttemptAnswer
                {
                    QuizQuestionId = question.Id,
                    QuestionText = question.Question,
                    SelectedOption = selected,
                    CorrectOption = option.CorrectOption,
                    IsCorrect = selected == option.CorrectOption
                });
            }

            var correct = answers.Count(a => a.IsCorrect);
            var attempt = new QuizAttempt
            {
                UserId = userId,
                LessonId = lessonId,
                AttemptNumber = (await ctx.QuizAttempts
                    .Where(a => a.UserId == userId && a.LessonId == lessonId)
                    .MaxAsync(a => (int?)a.AttemptNumber) ?? 0) + 1,
                TotalQuestions = questions.Count,
                CorrectAnswers = correct,
                ScorePercent = correct * 100 / questions.Count,
                SubmittedAt = DateTime.UtcNow,
                Answers = answers
            };
            ctx.QuizAttempts.Add(attempt);
            if (!await ctx.LessonCompletions.AnyAsync(c => c.UserId == userId && c.LessonId == lessonId))
                ctx.LessonCompletions.Add(new LessonCompletion { UserId = userId, LessonId = lessonId, CompletedAt = DateTime.UtcNow });
            await ctx.SaveChangesAsync();
            await RecalculateEnrollmentAsync(ctx, courseId, userId);
            await transaction.CommitAsync();
            attempt.Lesson = lesson;
            return CourseRepository.MapAttempt(attempt);
        }

        public async Task<List<QuizAttemptViewModel>> GetQuizAttemptsAsync(long lessonId, string userId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var attempts = await ctx.QuizAttempts.Include(a => a.Lesson).Include(a => a.Answers)
                .Where(a => a.LessonId == lessonId && a.UserId == userId)
                .OrderByDescending(a => a.AttemptNumber).AsNoTracking().ToListAsync();
            return attempts.Select(CourseRepository.MapAttempt).ToList();
        }

        public async Task<CourseReviewViewModel> UpsertReviewAsync(long courseId, string userId, CourseReviewEditModel model)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            await RequireActiveEnrollmentAsync(ctx, courseId, userId);
            var review = await ctx.CourseReviews.Include(r => r.User)
                .FirstOrDefaultAsync(r => r.CourseId == courseId && r.UserId == userId);
            if (review == null)
            {
                review = new CourseReview
                {
                    CourseId = courseId,
                    UserId = userId,
                    Rating = model.Rating,
                    Comment = model.Comment?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };
                ctx.CourseReviews.Add(review);
            }
            else
            {
                review.Rating = model.Rating;
                review.Comment = model.Comment?.Trim();
            }
            try
            {
                await ctx.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ctx.ChangeTracker.Clear();
                review = await ctx.CourseReviews.Include(r => r.User)
                    .SingleAsync(r => r.CourseId == courseId && r.UserId == userId);
                review.Rating = model.Rating;
                review.Comment = model.Comment?.Trim();
                await ctx.SaveChangesAsync();
            }
            if (review.User == null)
                await ctx.Entry(review).Reference(r => r.User).LoadAsync();
            var displayName = review.User?.FullName ?? review.User?.UserName ?? "Học viên";
            return new CourseReviewViewModel
            {
                Id = review.Id,
                UserId = review.UserId,
                UserDisplayName = displayName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                IsCurrentUser = true
            };
        }

        private static IQueryable<Course> PublishedGraph(AppDbContext ctx)
            => ctx.Courses.Where(c => c.Status == CourseStatus.Published)
                .Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.QuizQuestions).ThenInclude(q => q.Option)
                .Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Completions)
                .Include(c => c.Enrollments)
                .Include(c => c.Reviews).ThenInclude(r => r.User);

        private static CourseCatalogItemViewModel MapCatalog(Course course, string? userId)
        {
            var lessonIds = course.Sections.SelectMany(s => s.Lessons).Select(l => l.Id).ToHashSet();
            var enrollment = course.Enrollments.FirstOrDefault(e => e.UserId == userId && e.Status != EnrollmentStatus.Cancelled);
            var completed = userId == null ? 0 : course.Sections.SelectMany(s => s.Lessons)
                .Count(l => l.Completions.Any(c => c.UserId == userId));
            var total = lessonIds.Count;
            return new CourseCatalogItemViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                ThumbnailUrl = course.ThumbnailUrl,
                Level = course.Level,
                SectionCount = course.Sections.Count,
                TotalLessons = total,
                TotalDuration = course.Sections.SelectMany(s => s.Lessons).Sum(l => l.Duration ?? 0),
                EnrollmentCount = course.Enrollments.Count(e => e.Status != EnrollmentStatus.Cancelled),
                ReviewCount = course.Reviews.Count,
                AverageRating = course.Reviews.Count == 0 ? 0 : course.Reviews.Average(r => r.Rating),
                IsEnrolled = enrollment != null,
                EnrollmentStatus = enrollment?.Status,
                CompletedLessons = completed,
                ProgressPercent = total == 0 ? 0 : completed * 100 / total
            };
        }

        private static void CopyCatalog(CourseCatalogItemViewModel source, CourseCatalogItemViewModel target)
        {
            target.Id = source.Id;
            target.Title = source.Title;
            target.Description = source.Description;
            target.ThumbnailUrl = source.ThumbnailUrl;
            target.Level = source.Level;
            target.SectionCount = source.SectionCount;
            target.TotalLessons = source.TotalLessons;
            target.TotalDuration = source.TotalDuration;
            target.EnrollmentCount = source.EnrollmentCount;
            target.ReviewCount = source.ReviewCount;
            target.AverageRating = source.AverageRating;
            target.IsEnrolled = source.IsEnrolled;
            target.EnrollmentStatus = source.EnrollmentStatus;
            target.CompletedLessons = source.CompletedLessons;
            target.ProgressPercent = source.ProgressPercent;
        }

        private static QuizQuestionLearningViewModel MapLearnerQuestion(QuizQuestion question, int displayOrder)
        {
            var result = new QuizQuestionLearningViewModel
            {
                Id = question.Id,
                Question = question.Question,
                Type = question.Type,
                OrderIndex = displayOrder
            };
            if (question.Option == null) return result;
            result.Options.Add(new QuizOptionViewModel { Key = "1", Text = question.Option.OptionText1 });
            result.Options.Add(new QuizOptionViewModel { Key = "2", Text = question.Option.OptionText2 });
            result.Options.Add(new QuizOptionViewModel { Key = "3", Text = question.Option.OptionText3 });
            if (!string.IsNullOrWhiteSpace(question.Option.OptionText4))
                result.Options.Add(new QuizOptionViewModel { Key = "4", Text = question.Option.OptionText4 });
            return result;
        }

        private static async Task<CourseEnrollment> RequireActiveEnrollmentAsync(AppDbContext ctx, long courseId, string userId)
            => await ctx.CourseEnrollments.FirstOrDefaultAsync(e =>
                   e.CourseId == courseId && e.UserId == userId && e.Status != EnrollmentStatus.Cancelled)
               ?? throw new UnauthorizedAccessException("Bạn cần đăng ký khóa học để thực hiện thao tác này.");

        private static async Task RecalculateEnrollmentAsync(AppDbContext ctx, long courseId, string userId)
        {
            var enrollment = await ctx.CourseEnrollments.FirstAsync(e => e.CourseId == courseId && e.UserId == userId);
            if (enrollment.Status == EnrollmentStatus.Cancelled) return;
            var lessonIds = await ctx.CourseLessons.Where(l => l.Section.CourseId == courseId).Select(l => l.Id).ToListAsync();
            var completed = await ctx.LessonCompletions.CountAsync(c =>
                c.UserId == userId && c.LessonId != null && lessonIds.Contains(c.LessonId.Value));
            enrollment.Status = lessonIds.Count > 0 && completed == lessonIds.Count
                ? EnrollmentStatus.Completed
                : EnrollmentStatus.Active;
            await ctx.SaveChangesAsync();
        }
    }
}
