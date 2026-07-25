using CoreLTToeic.Application.Interfaces.Pattern;
using CoreLTToeic.Application.Models.EditModels;
using CoreLTToeic.Application.Models.ViewModels;
using CoreLTToeic.Domain.Entities;

namespace CoreLTToeic.Application.Interfaces.IRepository
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<List<CourseViewModel>> GetAllWithSectionsAsync();
        Task<CourseViewModel?> GetByIdWithSectionsAsync(long id);

        Task<CourseSectionViewModel> AddSectionAsync(long courseId, CourseSectionEditModel model);
        Task<CourseSectionViewModel> UpdateSectionAsync(long sectionId, CourseSectionEditModel model);
        Task MoveSectionAsync(long sectionId, int direction);
        Task DeleteSectionAsync(long sectionId);

        Task<CourseLessonViewModel> AddLessonAsync(long sectionId, CourseLessonEditModel model);
        Task<CourseLessonViewModel> UpdateLessonAsync(long lessonId, CourseLessonEditModel model);
        Task MoveLessonAsync(long lessonId, int direction);
        Task DeleteLessonAsync(long lessonId);

        Task<QuizQuestionAdminViewModel> AddQuizQuestionAsync(long lessonId, QuizQuestionEditModel model);
        Task<QuizQuestionAdminViewModel> UpdateQuizQuestionAsync(long questionId, QuizQuestionEditModel model);
        Task DeleteQuizQuestionAsync(long questionId);

        Task<List<CourseEnrollmentAdminViewModel>> GetEnrollmentsAsync(long courseId);
        Task UpdateEnrollmentStatusAsync(long enrollmentId, EnrollmentStatusEditModel model);
        Task<List<QuizAttemptViewModel>> GetQuizAttemptsAsync(long courseId, string? userId = null);
        Task<List<CourseReviewViewModel>> GetReviewsAsync(long courseId);
        Task DeleteReviewAsync(long reviewId);

        Task<bool> HasCourseLearnerDataAsync(long courseId);
        Task<bool> HasLessonLearnerDataAsync(long lessonId);
        Task RecalculateCourseEnrollmentsAsync(long courseId);
    }
}
