using CoreLTToeic.Application.Models.EditModels;
using CoreLTToeic.Application.Models.ViewModels;

namespace CoreLTToeic.Application.Interfaces.IRepository
{
    public interface ICourseLearningRepository
    {
        Task<List<CourseCatalogItemViewModel>> GetPublishedCoursesAsync(string? userId);
        Task<CourseDetailViewModel?> GetCourseDetailsAsync(long courseId, string? userId, bool includeLearningContent);
        Task<List<CourseCatalogItemViewModel>> GetMyCoursesAsync(string userId);
        Task EnrollAsync(long courseId, string userId);
        Task CompleteLessonAsync(long courseId, long lessonId, string userId);
        Task<QuizAttemptViewModel> SubmitQuizAsync(long courseId, long lessonId, string userId, QuizSubmissionEditModel model);
        Task<List<QuizAttemptViewModel>> GetQuizAttemptsAsync(long lessonId, string userId);
        Task<CourseReviewViewModel> UpsertReviewAsync(long courseId, string userId, CourseReviewEditModel model);
    }
}
