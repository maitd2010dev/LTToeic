namespace CoreLTToeic.Domain.Entities
{
    public class QuizAttemptAnswer
    {
        public long Id { get; set; }
        public long QuizAttemptId { get; set; }
        public QuizAttempt QuizAttempt { get; set; } = null!;
        public long? QuizQuestionId { get; set; }
        public QuizQuestion? QuizQuestion { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string SelectedOption { get; set; } = string.Empty;
        public string CorrectOption { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
