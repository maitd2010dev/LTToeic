using CoreLTToeic.Domain.Enums;

namespace CoreLTToeic.Application.Models.ViewModels
{
    public class CourseCatalogItemViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ThumbnailUrl { get; set; }
        public CourseLevel Level { get; set; }
        public int SectionCount { get; set; }
        public int TotalLessons { get; set; }
        public int TotalDuration { get; set; }
        public int EnrollmentCount { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
        public bool IsEnrolled { get; set; }
        public EnrollmentStatus? EnrollmentStatus { get; set; }
        public int CompletedLessons { get; set; }
        public int ProgressPercent { get; set; }
    }

    public class CourseDetailViewModel : CourseCatalogItemViewModel
    {
        public string? Objective { get; set; }
        public string? PreviewVideoUrl { get; set; }
        public List<CourseSectionLearningViewModel> Sections { get; set; } = new();
        public List<CourseReviewViewModel> Reviews { get; set; } = new();
    }

    public class CourseSectionLearningViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int CompletedLessons { get; set; }
        public List<CourseLessonLearningViewModel> Lessons { get; set; } = new();
    }

    public class CourseLessonLearningViewModel
    {
        public long Id { get; set; }
        public long SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public LessonType Type { get; set; }
        public int Duration { get; set; }
        public int OrderIndex { get; set; }
        public bool IsFree { get; set; }
        public bool CanAccess { get; set; }
        public bool IsCompleted { get; set; }
        public string? Content { get; set; }
        public string? VideoUrl { get; set; }
        public List<QuizQuestionLearningViewModel> QuizQuestions { get; set; } = new();
    }

    public class QuizQuestionLearningViewModel
    {
        public long Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string? Type { get; set; }
        public int OrderIndex { get; set; }
        public List<QuizOptionViewModel> Options { get; set; } = new();
    }

    public class QuizOptionViewModel
    {
        public string Key { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    public class CourseReviewViewModel
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    public class QuizAttemptViewModel
    {
        public long Id { get; set; }
        public long LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int ScorePercent { get; set; }
        public DateTime SubmittedAt { get; set; }
        public List<QuizAttemptAnswerViewModel> Answers { get; set; } = new();
    }

    public class QuizAttemptAnswerViewModel
    {
        public long? QuizQuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string SelectedOption { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
