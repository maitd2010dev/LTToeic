using CoreLTToeic.Domain.Enums;

namespace CoreLTToeic.Application.Models.ViewModels;

public class UserResultViewModel
{
    public long Id { get; set; }
    public long TestId { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public int SkippedAnswers { get; set; }
    public int ListeningCorrects { get; set; }
    public int ReadingCorrects { get; set; }
    public int TotalListeningQuestions { get; set; }
    public int TotalReadingQuestions { get; set; }
    public int ListeningScore { get; set; }
    public int ReadingScore { get; set; }
    public int TotalScore { get; set; }
    public float Accuracy { get; set; }
    public int CompletionTime { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TestMode TestMode { get; set; }
    public AttemptStatus AttemptStatus { get; set; }
    public string? UserId { get; set; }
    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }
    public string TestTitle { get; set; } = string.Empty;
    public List<UserAnswerViewModel> Answers { get; set; } = [];
}
