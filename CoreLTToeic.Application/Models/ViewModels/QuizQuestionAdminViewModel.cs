namespace CoreLTToeic.Application.Models.ViewModels
{
    public class QuizQuestionAdminViewModel
    {
        public long Id { get; set; }
        public long? LessonId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string? Type { get; set; }
        public int? OrderIndex { get; set; }
        public string OptionText1 { get; set; } = string.Empty;
        public string OptionText2 { get; set; } = string.Empty;
        public string OptionText3 { get; set; } = string.Empty;
        public string? OptionText4 { get; set; }
        public string CorrectOption { get; set; } = string.Empty;
    }
}
