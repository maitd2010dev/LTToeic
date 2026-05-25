namespace CoreLTToeic.Domain.Entities
{
    public class QuizQuestionOption
    {
        public long Id { get; set; }
        public long? QuizQuestionId { get; set; }
        public QuizQuestion? QuizQuestion { get; set; }
        public string OptionText1 { get; set; } = string.Empty;
        public string OptionText2 { get; set; } = string.Empty;
        public string OptionText3 { get; set; } = string.Empty;
        public string? OptionText4 { get; set; }
        public string CorrectOption { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
