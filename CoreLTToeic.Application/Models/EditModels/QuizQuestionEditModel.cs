namespace CoreLTToeic.Application.Models.EditModels
{
    public class QuizQuestionEditModel
    {
        public long Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string? Type { get; set; } = "multiple_choice";
        public int? OrderIndex { get; set; }
        public string OptionText1 { get; set; } = string.Empty;
        public string OptionText2 { get; set; } = string.Empty;
        public string OptionText3 { get; set; } = string.Empty;
        public string? OptionText4 { get; set; }
        public string CorrectOption { get; set; } = "1";
    }
}
