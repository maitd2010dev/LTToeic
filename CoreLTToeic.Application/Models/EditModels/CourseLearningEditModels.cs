using CoreLTToeic.Domain.Enums;

namespace CoreLTToeic.Application.Models.EditModels
{
    public class QuizSubmissionEditModel
    {
        public Dictionary<long, string> Answers { get; set; } = new();
    }

    public class CourseReviewEditModel
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class EnrollmentStatusEditModel
    {
        public EnrollmentStatus Status { get; set; }
    }
}
