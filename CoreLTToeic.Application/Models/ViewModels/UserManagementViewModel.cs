namespace CoreLTToeic.Application.Models.ViewModels;

public class UserManagementViewModel
{
    public string Id { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool IsLocked { get; set; }
    public string Role { get; set; } = "User";
    public int ExamCount { get; set; }
}
