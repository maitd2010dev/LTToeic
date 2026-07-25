using CoreLTToeic.Application.Interfaces.IRepository;
using CoreLTToeic.Application.Interfaces.IService;
using CoreLTToeic.Application.Models.EditModels;
using CoreLTToeic.Application.Models.ViewModels;

namespace CoreLTToeic.Application.Business
{
    public class CourseLearningService : ICourseLearningService
    {
        private readonly ICourseLearningRepository _repository;

        public CourseLearningService(ICourseLearningRepository repository)
        {
            _repository = repository;
        }

        public Task<List<CourseCatalogItemViewModel>> GetPublishedCoursesAsync(string? userId)
            => _repository.GetPublishedCoursesAsync(userId);

        public Task<CourseDetailViewModel?> GetCourseDetailsAsync(long courseId, string? userId)
            => _repository.GetCourseDetailsAsync(courseId, userId, false);

        public Task<CourseDetailViewModel?> GetLearningCourseAsync(long courseId, string userId)
        {
            RequireUser(userId);
            return _repository.GetCourseDetailsAsync(courseId, userId, true);
        }

        public Task<List<CourseCatalogItemViewModel>> GetMyCoursesAsync(string userId)
        {
            RequireUser(userId);
            return _repository.GetMyCoursesAsync(userId);
        }

        public Task EnrollAsync(long courseId, string userId)
        {
            RequireUser(userId);
            return _repository.EnrollAsync(courseId, userId);
        }

        public Task CompleteLessonAsync(long courseId, long lessonId, string userId)
        {
            RequireUser(userId);
            return _repository.CompleteLessonAsync(courseId, lessonId, userId);
        }

        public Task<QuizAttemptViewModel> SubmitQuizAsync(long courseId, long lessonId, string userId, QuizSubmissionEditModel model)
        {
            RequireUser(userId);
            if (model.Answers.Count == 0)
                throw new InvalidOperationException("Vui lòng trả lời tất cả câu hỏi.");
            return _repository.SubmitQuizAsync(courseId, lessonId, userId, model);
        }

        public Task<List<QuizAttemptViewModel>> GetQuizAttemptsAsync(long lessonId, string userId)
        {
            RequireUser(userId);
            return _repository.GetQuizAttemptsAsync(lessonId, userId);
        }

        public Task<CourseReviewViewModel> UpsertReviewAsync(long courseId, string userId, CourseReviewEditModel model)
        {
            RequireUser(userId);
            if (model.Rating is < 1 or > 5)
                throw new InvalidOperationException("Đánh giá phải từ 1 đến 5 sao.");
            if (model.Comment?.Length > 2000)
                throw new InvalidOperationException("Nội dung đánh giá không được vượt quá 2.000 ký tự.");
            return _repository.UpsertReviewAsync(courseId, userId, model);
        }

        private static void RequireUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedAccessException("Vui lòng đăng nhập để tiếp tục.");
        }
    }
}
