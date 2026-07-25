namespace CoreLTToeic.Domain.Entities
{
    public class QuizAttempt
    {
        public long Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;
        public long LessonId { get; set; }
        public CourseLesson Lesson { get; set; } = null!;
        public int AttemptNumber { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int ScorePercent { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
    }
}
