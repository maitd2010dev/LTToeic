using CoreLTToeic.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CoreLTToeic.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime? LastLogin { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Avatar { get; set; }
        public string? Address { get; set; }
        public string? Education { get; set; }
        public string? Occupation { get; set; }
        public string? EnglishLevel { get; set; }
        public int? TargetScore { get; set; }
        public ICollection<UserResult> UserResults { get; set; } = new List<UserResult>();
        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    }
}
