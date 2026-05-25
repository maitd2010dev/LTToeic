namespace CoreLTToeic.Application.Models.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalTests { get; set; }
    public int TotalCompletedResults { get; set; }
    public int AverageTotalScore { get; set; }
    public int TotalCourses { get; set; }
    public List<DailyActivityViewModel> DailyActivity { get; set; } = [];
    public List<TopTestViewModel> TopTests { get; set; } = [];
}

public class DailyActivityViewModel
{
    public string Date { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TopTestViewModel
{
    public string TestTitle { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
}
