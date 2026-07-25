using CoreLTToeic.Domain.Enums;

namespace CoreLTToeic.Domain.Entities
{
    public class CourseLesson
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public LessonType? Type { get; set; }
        public int? Duration { get; set; }
        public int? OrderIndex { get; set; }
        public bool? IsFree { get; set; }
        public string? Content { get; set; }
        public string? VideoUrl { get; set; }

        public long SectionId { get; set; }
        public CourseSection Section { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<LessonCompletion> Completions { get; set; } = new List<LessonCompletion>();
        public ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    }
}
