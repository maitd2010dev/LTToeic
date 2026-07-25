using CoreLTToeic.Domain.Enums;

namespace CoreLTToeic.Application.Models.ViewModels
{
    public class CourseEnrollmentAdminViewModel
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public EnrollmentStatus Status { get; set; }
        public DateTime EnrolledAt { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public int ProgressPercent { get; set; }
        public int QuizAttemptCount { get; set; }
        public int? BestQuizScore { get; set; }
    }
}
