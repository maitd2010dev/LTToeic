using AutoMapper;
using CoreLTToeic.Application.Interfaces.IRepository;
using CoreLTToeic.Application.Models.EditModels;
using CoreLTToeic.Application.Models.ViewModels;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Domain.Enums;
using CoreLTToeic.Infrastructure.Context;
using CoreLTToeic.Infrastructure.Pattern;
using Microsoft.EntityFrameworkCore;

namespace CoreLTToeic.Infrastructure.Repositories
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IMapper _mapper;

        public CourseRepository(IDbContextFactory<AppDbContext> factory, IMapper mapper) : base(factory)
        {
            _factory = factory;
            _mapper = mapper;
        }

        public async Task<List<CourseViewModel>> GetAllWithSectionsAsync()
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var courses = await CourseGraph(ctx)
                .OrderByDescending(c => c.Id)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<List<CourseViewModel>>(courses);
        }

        public async Task<CourseViewModel?> GetByIdWithSectionsAsync(long id)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var course = await CourseGraph(ctx)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            return course == null ? null : _mapper.Map<CourseViewModel>(course);
        }

        public async Task<CourseSectionViewModel> AddSectionAsync(long courseId, CourseSectionEditModel model)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            if (!await ctx.Courses.AnyAsync(c => c.Id == courseId))
                throw new KeyNotFoundException($"Không tìm thấy khóa học #{courseId}");

            var section = _mapper.Map<CourseSection>(model);
            section.Id = 0;
            section.CourseId = courseId;
            section.OrderIndex = (await ctx.CourseSections
                .Where(s => s.CourseId == courseId)
                .MaxAsync(s => (int?)s.OrderIndex) ?? 0) + 1;
            ctx.CourseSections.Add(section);
            await ctx.SaveChangesAsync();
            await RecalculateCourseEnrollmentsAsync(courseId);
            return _mapper.Map<CourseSectionViewModel>(section);
        }

        public async Task<CourseSectionViewModel> UpdateSectionAsync(long sectionId, CourseSectionEditModel model)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var section = await ctx.CourseSections.FindAsync(sectionId)
                ?? throw new KeyNotFoundException($"Không tìm thấy chương #{sectionId}");
            section.Title = model.Title.Trim();
            section.UpdatedAt = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
            return _mapper.Map<CourseSectionViewModel>(section);
        }

        public async Task MoveSectionAsync(long sectionId, int direction)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var current = await ctx.CourseSections.FindAsync(sectionId)
                ?? throw new KeyNotFoundException($"Không tìm thấy chương #{sectionId}");
            var items = await ctx.CourseSections.Where(s => s.CourseId == current.CourseId)
                .OrderBy(s => s.OrderIndex).ThenBy(s => s.Id).ToListAsync();
            SwapOrder(items, current, direction, item => item.OrderIndex ?? 0, (item, value) => item.OrderIndex = value);
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteSectionAsync(long sectionId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var section = await ctx.CourseSections.Include(s => s.Course).Include(s => s.Lessons).FirstOrDefaultAsync(s => s.Id == sectionId);
            if (section == null) return;
            var lessonIds = section.Lessons.Select(l => l.Id).ToList();
            if (await ctx.LessonCompletions.AnyAsync(c => c.LessonId != null && lessonIds.Contains(c.LessonId.Value)) ||
                await ctx.QuizAttempts.AnyAsync(a => lessonIds.Contains(a.LessonId)))
                throw new InvalidOperationException("Chương có dữ liệu tiến độ học viên và không thể xóa.");
            if (section.Course.Status == CourseStatus.Published)
            {
                var remainingLessons = await ctx.CourseLessons.CountAsync(l => l.Section.CourseId == section.CourseId && l.SectionId != sectionId);
                if (remainingLessons == 0)
                    throw new InvalidOperationException("Không thể xóa nội dung cuối cùng của khóa học đang xuất bản. Hãy chuyển khóa học về bản nháp trước.");
            }
            var courseId = section.CourseId;
            ctx.CourseSections.Remove(section);
            await ctx.SaveChangesAsync();
            await RecalculateCourseEnrollmentsAsync(courseId);
        }

        public async Task<CourseLessonViewModel> AddLessonAsync(long sectionId, CourseLessonEditModel model)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var section = await ctx.CourseSections.Include(s => s.Course).FirstOrDefaultAsync(s => s.Id == sectionId)
                ?? throw new KeyNotFoundException($"Không tìm thấy chương #{sectionId}");
            if (section.Course.Status == CourseStatus.Published && model.Type == LessonType.Quiz)
                throw new InvalidOperationException("Hãy chuyển khóa học về bản nháp, thêm bài kiểm tra và cấu hình câu hỏi trước khi xuất bản lại.");
            var lesson = _mapper.Map<CourseLesson>(model);
            lesson.Id = 0;
            lesson.SectionId = sectionId;
            lesson.OrderIndex = (await ctx.CourseLessons.Where(l => l.SectionId == sectionId)
                .MaxAsync(l => (int?)l.OrderIndex) ?? 0) + 1;
            lesson.CreatedAt = lesson.UpdatedAt = DateTime.UtcNow;
            ctx.CourseLessons.Add(lesson);
            await ctx.SaveChangesAsync();
            await RecalculateCourseEnrollmentsAsync(section.CourseId);
            return _mapper.Map<CourseLessonViewModel>(lesson);
        }

        public async Task<CourseLessonViewModel> UpdateLessonAsync(long lessonId, CourseLessonEditModel model)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var lesson = await ctx.CourseLessons.Include(l => l.Section).ThenInclude(s => s.Course)
                .Include(l => l.QuizQuestions).ThenInclude(q => q.Option)
                .FirstOrDefaultAsync(l => l.Id == lessonId)
                ?? throw new KeyNotFoundException($"Không tìm thấy bài học #{lessonId}");
            if (lesson.Section.Course.Status == CourseStatus.Published && model.Type == LessonType.Quiz && lesson.QuizQuestions.Count == 0)
                throw new InvalidOperationException("Bài kiểm tra trong khóa học đang xuất bản phải có ít nhất một câu hỏi.");
            lesson.Title = model.Title.Trim();
            lesson.Description = model.Description;
            lesson.Type = model.Type;
            lesson.Duration = model.Duration;
            lesson.IsFree = model.IsFree ?? false;
            lesson.Content = model.Type == LessonType.Text ? model.Content : null;
            lesson.VideoUrl = model.Type == LessonType.Video ? model.VideoUrl : null;
            lesson.UpdatedAt = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
            return _mapper.Map<CourseLessonViewModel>(lesson);
        }

        public async Task MoveLessonAsync(long lessonId, int direction)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var current = await ctx.CourseLessons.FindAsync(lessonId)
                ?? throw new KeyNotFoundException($"Không tìm thấy bài học #{lessonId}");
            var items = await ctx.CourseLessons.Where(l => l.SectionId == current.SectionId)
                .OrderBy(l => l.OrderIndex).ThenBy(l => l.Id).ToListAsync();
            SwapOrder(items, current, direction, item => item.OrderIndex ?? 0, (item, value) => item.OrderIndex = value);
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteLessonAsync(long lessonId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var lesson = await ctx.CourseLessons.Include(l => l.Section).FirstOrDefaultAsync(l => l.Id == lessonId);
            if (lesson == null) return;
            var courseId = lesson.Section.CourseId;
            var courseStatus = await ctx.Courses.Where(c => c.Id == courseId).Select(c => c.Status).SingleAsync();
            if (courseStatus == CourseStatus.Published &&
                await ctx.CourseLessons.CountAsync(l => l.Section.CourseId == courseId) <= 1)
                throw new InvalidOperationException("Không thể xóa bài học cuối cùng của khóa học đang xuất bản. Hãy chuyển khóa học về bản nháp trước.");
            ctx.CourseLessons.Remove(lesson);
            await ctx.SaveChangesAsync();
            await RecalculateCourseEnrollmentsAsync(courseId);
        }

        public async Task<QuizQuestionAdminViewModel> AddQuizQuestionAsync(long lessonId, QuizQuestionEditModel model)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var lesson = await ctx.CourseLessons.FindAsync(lessonId)
                ?? throw new KeyNotFoundException($"Không tìm thấy bài học #{lessonId}");
            if (lesson.Type != LessonType.Quiz)
                throw new InvalidOperationException("Chỉ bài học kiểm tra mới có câu hỏi.");

            var question = BuildQuestion(model);
            question.LessonId = lessonId;
            question.OrderIndex = (await ctx.QuizQuestions.Where(q => q.LessonId == lessonId)
                .MaxAsync(q => (int?)q.OrderIndex) ?? 0) + 1;
            ctx.QuizQuestions.Add(question);
            await ctx.SaveChangesAsync();
            return _mapper.Map<QuizQuestionAdminViewModel>(question);
        }

        public async Task<QuizQuestionAdminViewModel> UpdateQuizQuestionAsync(long questionId, QuizQuestionEditModel model)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var question = await ctx.QuizQuestions.Include(q => q.Option).FirstOrDefaultAsync(q => q.Id == questionId)
                ?? throw new KeyNotFoundException($"Không tìm thấy câu hỏi #{questionId}");
            question.Question = model.Question.Trim();
            question.Type = model.Type;
            question.UpdatedAt = DateTime.UtcNow;
            question.Option ??= new QuizQuestionOption { QuizQuestionId = question.Id };
            ApplyOption(question.Option, model);
            await ctx.SaveChangesAsync();
            return _mapper.Map<QuizQuestionAdminViewModel>(question);
        }

        public async Task DeleteQuizQuestionAsync(long questionId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var question = await ctx.QuizQuestions.Include(q => q.Lesson).ThenInclude(l => l!.Section).ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(q => q.Id == questionId);
            if (question == null) return;
            if (question.Lesson?.Section.Course.Status == CourseStatus.Published &&
                await ctx.QuizQuestions.CountAsync(q => q.LessonId == question.LessonId) <= 1)
                throw new InvalidOperationException("Không thể xóa câu hỏi cuối cùng của bài kiểm tra đang xuất bản.");
            ctx.QuizQuestions.Remove(question);
            await ctx.SaveChangesAsync();
        }

        public async Task<List<CourseEnrollmentAdminViewModel>> GetEnrollmentsAsync(long courseId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var totalLessons = await ctx.CourseLessons.CountAsync(l => l.Section.CourseId == courseId);
            var enrollments = await ctx.CourseEnrollments.Include(e => e.User)
                .Where(e => e.CourseId == courseId).OrderByDescending(e => e.EnrolledAt).AsNoTracking().ToListAsync();
            var result = new List<CourseEnrollmentAdminViewModel>();
            foreach (var enrollment in enrollments)
            {
                var completed = await ctx.LessonCompletions.CountAsync(c =>
                    c.UserId == enrollment.UserId && c.Lesson != null && c.Lesson.Section.CourseId == courseId);
                var attempts = await ctx.QuizAttempts.Where(a => a.UserId == enrollment.UserId && a.Lesson.Section.CourseId == courseId)
                    .Select(a => a.ScorePercent).ToListAsync();
                result.Add(new CourseEnrollmentAdminViewModel
                {
                    Id = enrollment.Id,
                    UserId = enrollment.UserId,
                    UserName = enrollment.User.UserName ?? string.Empty,
                    FullName = enrollment.User.FullName,
                    Email = enrollment.User.Email,
                    Status = enrollment.Status ?? EnrollmentStatus.Active,
                    EnrolledAt = enrollment.EnrolledAt,
                    CompletedLessons = completed,
                    TotalLessons = totalLessons,
                    ProgressPercent = totalLessons == 0 ? 0 : completed * 100 / totalLessons,
                    QuizAttemptCount = attempts.Count,
                    BestQuizScore = attempts.Count == 0 ? null : attempts.Max()
                });
            }
            return result;
        }

        public async Task UpdateEnrollmentStatusAsync(long enrollmentId, EnrollmentStatusEditModel model)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var enrollment = await ctx.CourseEnrollments.FindAsync(enrollmentId)
                ?? throw new KeyNotFoundException($"Không tìm thấy đăng ký #{enrollmentId}");
            enrollment.Status = model.Status;
            await ctx.SaveChangesAsync();
        }

        public async Task<List<QuizAttemptViewModel>> GetQuizAttemptsAsync(long courseId, string? userId = null)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var query = ctx.QuizAttempts.Include(a => a.Lesson).Include(a => a.Answers)
                .Where(a => a.Lesson.Section.CourseId == courseId);
            if (!string.IsNullOrWhiteSpace(userId)) query = query.Where(a => a.UserId == userId);
            return (await query.OrderByDescending(a => a.SubmittedAt).AsNoTracking().ToListAsync())
                .Select(MapAttempt).ToList();
        }

        public async Task<List<CourseReviewViewModel>> GetReviewsAsync(long courseId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            return await ctx.CourseReviews.Include(r => r.User).Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt).AsNoTracking()
                .Select(r => new CourseReviewViewModel
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserDisplayName = r.User.FullName ?? r.User.UserName ?? "Học viên",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToListAsync();
        }

        public async Task DeleteReviewAsync(long reviewId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var review = await ctx.CourseReviews.FindAsync(reviewId);
            if (review == null) return;
            ctx.CourseReviews.Remove(review);
            await ctx.SaveChangesAsync();
        }

        public async Task<bool> HasCourseLearnerDataAsync(long courseId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            return await ctx.CourseEnrollments.AnyAsync(e => e.CourseId == courseId)
                || await ctx.LessonCompletions.AnyAsync(c => c.Lesson != null && c.Lesson.Section.CourseId == courseId)
                || await ctx.QuizAttempts.AnyAsync(a => a.Lesson.Section.CourseId == courseId);
        }

        public async Task<bool> HasLessonLearnerDataAsync(long lessonId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            return await ctx.LessonCompletions.AnyAsync(c => c.LessonId == lessonId)
                || await ctx.QuizAttempts.AnyAsync(a => a.LessonId == lessonId);
        }

        public async Task RecalculateCourseEnrollmentsAsync(long courseId)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            var lessonIds = await ctx.CourseLessons.Where(l => l.Section.CourseId == courseId).Select(l => l.Id).ToListAsync();
            var enrollments = await ctx.CourseEnrollments.Where(e => e.CourseId == courseId && e.Status != EnrollmentStatus.Cancelled).ToListAsync();
            foreach (var enrollment in enrollments)
            {
                var completeCount = await ctx.LessonCompletions.CountAsync(c =>
                    c.UserId == enrollment.UserId && c.LessonId != null && lessonIds.Contains(c.LessonId.Value));
                enrollment.Status = lessonIds.Count > 0 && completeCount == lessonIds.Count
                    ? EnrollmentStatus.Completed
                    : EnrollmentStatus.Active;
            }
            await ctx.SaveChangesAsync();
        }

        private static IQueryable<Course> CourseGraph(AppDbContext ctx)
            => ctx.Courses
                .Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.QuizQuestions).ThenInclude(q => q.Option)
                .Include(c => c.Enrollments)
                .Include(c => c.Reviews);

        private static QuizQuestion BuildQuestion(QuizQuestionEditModel model)
        {
            var question = new QuizQuestion
            {
                Question = model.Question.Trim(),
                Type = model.Type,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Option = new QuizQuestionOption()
            };
            ApplyOption(question.Option, model);
            return question;
        }

        private static void ApplyOption(QuizQuestionOption option, QuizQuestionEditModel model)
        {
            option.OptionText1 = model.OptionText1.Trim();
            option.OptionText2 = model.OptionText2.Trim();
            option.OptionText3 = model.OptionText3.Trim();
            option.OptionText4 = string.IsNullOrWhiteSpace(model.OptionText4) ? null : model.OptionText4.Trim();
            option.CorrectOption = model.CorrectOption;
            option.UpdatedAt = DateTime.UtcNow;
        }

        private static void SwapOrder<T>(IList<T> items, T current, int direction, Func<T, int> read, Action<T, int> write)
        {
            if (direction is not (-1 or 1))
                throw new ArgumentOutOfRangeException(nameof(direction), "Direction must be -1 or 1.");
            for (var i = 0; i < items.Count; i++) write(items[i], i + 1);
            var index = items.IndexOf(current);
            var target = index + direction;
            if (index < 0 || target < 0 || target >= items.Count) return;
            var currentOrder = read(items[index]);
            var targetOrder = read(items[target]);
            write(items[index], targetOrder);
            write(items[target], currentOrder);
        }

        internal static QuizAttemptViewModel MapAttempt(QuizAttempt attempt) => new()
        {
            Id = attempt.Id,
            LessonId = attempt.LessonId,
            LessonTitle = attempt.Lesson?.Title ?? string.Empty,
            AttemptNumber = attempt.AttemptNumber,
            TotalQuestions = attempt.TotalQuestions,
            CorrectAnswers = attempt.CorrectAnswers,
            ScorePercent = attempt.ScorePercent,
            SubmittedAt = attempt.SubmittedAt,
            Answers = attempt.Answers.Select(answer => new QuizAttemptAnswerViewModel
            {
                QuizQuestionId = answer.QuizQuestionId,
                QuestionText = answer.QuestionText,
                SelectedOption = answer.SelectedOption,
                CorrectOption = answer.CorrectOption,
                IsCorrect = answer.IsCorrect
            }).ToList()
        };
    }
}
