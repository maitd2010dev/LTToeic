using CoreLTToeic.Domain.Enums;

namespace CoreLTToeic.Application.Models.ViewModels
{
    public class CourseViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Objective { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? PreviewVideoUrl { get; set; }
        public decimal Price { get; set; }
        public CourseLevel Level { get; set; }
        public CourseStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int SectionCount { get; set; }
        public int TotalLessons { get; set; }
        public int EnrollmentCount { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
        public List<CourseSectionViewModel> Sections { get; set; } = new();
    }
}
