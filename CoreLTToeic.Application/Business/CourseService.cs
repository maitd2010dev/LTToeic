using AutoMapper;
using CoreLTToeic.Application.Interfaces.IRepository;
using CoreLTToeic.Application.Interfaces.IService;
using CoreLTToeic.Application.Models.EditModels;
using CoreLTToeic.Application.Models.ViewModels;
using CoreLTToeic.Domain.Entities;

namespace CoreLTToeic.Application.Business
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepo;
        private readonly IMapper _mapper;

        public CourseService(ICourseRepository courseRepo, IMapper mapper)
        {
            _courseRepo = courseRepo;
            _mapper = mapper;
        }

        public async Task<List<CourseViewModel>> GetAllAsync()
            => await _courseRepo.GetAllWithSectionsAsync();

        public async Task<CourseViewModel?> GetByIdAsync(long id)
            => await _courseRepo.GetByIdWithSectionsAsync(id);

        public async Task<CourseViewModel> CreateAsync(CourseEditModel model, string? thumbnailPath)
        {
            ValidateGeneral(model);
            if (model.Status == Domain.Enums.CourseStatus.Published)
                throw new InvalidOperationException("Hãy tạo chương và bài học trước khi xuất bản khóa học.");

            var entity = _mapper.Map<Course>(model);
            entity.Price = 0;
            entity.ThumbnailUrl = thumbnailPath;
            entity.CreatedAt = DateTime.UtcNow;
            _courseRepo.Add(entity);
            await _courseRepo.SaveChangesAsync();
            return (await _courseRepo.GetByIdWithSectionsAsync(entity.Id))!;
        }

        public async Task<CourseViewModel> UpdateAsync(long id, CourseEditModel model, string? thumbnailPath)
        {
            ValidateGeneral(model);
            if (model.Status == Domain.Enums.CourseStatus.Published)
                await ValidatePublicationAsync(id);

            var all = await _courseRepo.GetAllBySearchAsync(c => c.Id == id);
            var entity = all.FirstOrDefault() ?? throw new KeyNotFoundException($"Không tìm thấy khóa học #{id}");
            _mapper.Map(model, entity);
            entity.Price = 0;
            entity.UpdatedAt = DateTime.UtcNow;
            if (thumbnailPath != null)
                entity.ThumbnailUrl = thumbnailPath;
            _courseRepo.Update(entity);
            await _courseRepo.SaveChangesAsync();
            return (await _courseRepo.GetByIdWithSectionsAsync(id))!;
        }

        public async Task DeleteAsync(long id)
        {
            if (await _courseRepo.HasCourseLearnerDataAsync(id))
                throw new InvalidOperationException("Khóa học đã có dữ liệu học viên. Hãy chuyển khóa học về bản nháp thay vì xóa.");

            var all = await _courseRepo.GetAllBySearchAsync(c => c.Id == id);
            var entity = all.FirstOrDefault() ?? throw new KeyNotFoundException($"Không tìm thấy khóa học #{id}");
            _courseRepo.Remove(entity);
            await _courseRepo.SaveChangesAsync();
        }

        public Task<CourseSectionViewModel> AddSectionAsync(long courseId, CourseSectionEditModel model)
        {
            ValidateTitle(model.Title, "Tên chương");
            return _courseRepo.AddSectionAsync(courseId, model);
        }

        public Task<CourseSectionViewModel> UpdateSectionAsync(long sectionId, CourseSectionEditModel model)
        {
            ValidateTitle(model.Title, "Tên chương");
            return _courseRepo.UpdateSectionAsync(sectionId, model);
        }

        public Task MoveSectionAsync(long sectionId, int direction)
            => _courseRepo.MoveSectionAsync(sectionId, direction);

        public Task DeleteSectionAsync(long sectionId)
            => _courseRepo.DeleteSectionAsync(sectionId);

        public Task<CourseLessonViewModel> AddLessonAsync(long sectionId, CourseLessonEditModel model)
        {
            ValidateLesson(model);
            return _courseRepo.AddLessonAsync(sectionId, model);
        }

        public Task<CourseLessonViewModel> UpdateLessonAsync(long lessonId, CourseLessonEditModel model)
        {
            ValidateLesson(model);
            return _courseRepo.UpdateLessonAsync(lessonId, model);
        }

        public Task MoveLessonAsync(long lessonId, int direction)
            => _courseRepo.MoveLessonAsync(lessonId, direction);

        public async Task DeleteLessonAsync(long lessonId)
        {
            if (await _courseRepo.HasLessonLearnerDataAsync(lessonId))
                throw new InvalidOperationException("Bài học đã có tiến độ hoặc lượt làm bài và không thể xóa.");
            await _courseRepo.DeleteLessonAsync(lessonId);
        }

        public Task<QuizQuestionAdminViewModel> AddQuizQuestionAsync(long lessonId, QuizQuestionEditModel model)
        {
            ValidateQuizQuestion(model);
            return _courseRepo.AddQuizQuestionAsync(lessonId, model);
        }

        public Task<QuizQuestionAdminViewModel> UpdateQuizQuestionAsync(long questionId, QuizQuestionEditModel model)
        {
            ValidateQuizQuestion(model);
            return _courseRepo.UpdateQuizQuestionAsync(questionId, model);
        }

        public Task DeleteQuizQuestionAsync(long questionId) => _courseRepo.DeleteQuizQuestionAsync(questionId);
        public Task<List<CourseEnrollmentAdminViewModel>> GetEnrollmentsAsync(long courseId) => _courseRepo.GetEnrollmentsAsync(courseId);
        public Task UpdateEnrollmentStatusAsync(long enrollmentId, EnrollmentStatusEditModel model) => _courseRepo.UpdateEnrollmentStatusAsync(enrollmentId, model);
        public Task<List<QuizAttemptViewModel>> GetQuizAttemptsAsync(long courseId, string? userId = null) => _courseRepo.GetQuizAttemptsAsync(courseId, userId);
        public Task<List<CourseReviewViewModel>> GetReviewsAsync(long courseId) => _courseRepo.GetReviewsAsync(courseId);
        public Task DeleteReviewAsync(long reviewId) => _courseRepo.DeleteReviewAsync(reviewId);

        private async Task ValidatePublicationAsync(long id)
        {
            var course = await _courseRepo.GetByIdWithSectionsAsync(id)
                ?? throw new KeyNotFoundException($"Không tìm thấy khóa học #{id}");

            if (course.Sections.Count == 0 || course.Sections.All(section => section.Lessons.Count == 0))
                throw new InvalidOperationException("Khóa học xuất bản phải có ít nhất một chương và một bài học.");
            if (course.Sections.Any(section => string.IsNullOrWhiteSpace(section.Title)))
                throw new InvalidOperationException("Mỗi chương phải có tên.");

            foreach (var lesson in course.Sections.SelectMany(section => section.Lessons))
            {
                if (lesson.Type == Domain.Enums.LessonType.Video && !IsHttpUrl(lesson.VideoUrl))
                    throw new InvalidOperationException($"Bài video “{lesson.Title}” cần URL HTTP/HTTPS hợp lệ.");
                if (lesson.Type == Domain.Enums.LessonType.Quiz &&
                    (lesson.QuizQuestions.Count == 0 || lesson.QuizQuestions.Any(question => !IsQuizQuestionComplete(question))))
                    throw new InvalidOperationException($"Bài kiểm tra “{lesson.Title}” cần ít nhất một câu hỏi hoàn chỉnh.");
            }
        }

        private static void ValidateGeneral(CourseEditModel model)
        {
            ValidateTitle(model.Title, "Tên khóa học");
            if (string.IsNullOrWhiteSpace(model.Description))
                throw new InvalidOperationException("Mô tả khóa học là bắt buộc.");
            if (!string.IsNullOrWhiteSpace(model.PreviewVideoUrl) && !IsHttpUrl(model.PreviewVideoUrl))
                throw new InvalidOperationException("URL video giới thiệu phải dùng HTTP hoặc HTTPS.");
        }

        private static void ValidateLesson(CourseLessonEditModel model)
        {
            ValidateTitle(model.Title, "Tên bài học");
            if (model.Type == Domain.Enums.LessonType.Video && !IsHttpUrl(model.VideoUrl))
                throw new InvalidOperationException("Bài học video cần URL HTTP/HTTPS hợp lệ.");
        }

        private static void ValidateQuizQuestion(QuizQuestionEditModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Question) ||
                string.IsNullOrWhiteSpace(model.OptionText1) ||
                string.IsNullOrWhiteSpace(model.OptionText2) ||
                string.IsNullOrWhiteSpace(model.OptionText3))
                throw new InvalidOperationException("Câu hỏi và ba lựa chọn đầu tiên là bắt buộc.");

            var allowed = string.IsNullOrWhiteSpace(model.OptionText4)
                ? new[] { "1", "2", "3" }
                : new[] { "1", "2", "3", "4" };
            if (!allowed.Contains(model.CorrectOption))
                throw new InvalidOperationException("Đáp án đúng phải trỏ đến một lựa chọn hiện có.");
        }

        private static bool IsQuizQuestionComplete(QuizQuestionAdminViewModel question)
            => !string.IsNullOrWhiteSpace(question.Question)
               && !string.IsNullOrWhiteSpace(question.OptionText1)
               && !string.IsNullOrWhiteSpace(question.OptionText2)
               && !string.IsNullOrWhiteSpace(question.OptionText3)
               && new[] { "1", "2", "3", "4" }.Contains(question.CorrectOption)
               && (question.CorrectOption != "4" || !string.IsNullOrWhiteSpace(question.OptionText4));

        private static void ValidateTitle(string? value, string field)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"{field} là bắt buộc.");
        }

        private static bool IsHttpUrl(string? value)
            => Uri.TryCreate(value, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
