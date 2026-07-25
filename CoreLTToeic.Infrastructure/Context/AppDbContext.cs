using CoreLTToeic.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreLTToeic.Infrastructure.Context
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> AppUser { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestCategory> TestCategories { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<QuestionGroup> QuestionGroups { get; set; }
        public DbSet<QuestionGroupImage> QuestionGroupImages { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<UserResult> UserResults { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<ReadingScoreConversion> ReadingScoreConversions { get; set; }
        public DbSet<ListeningScoreConversion> ListeningScoreConversions { get; set; }

        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseSection> CourseSections { get; set; }
        public DbSet<CourseLesson> CourseLessons { get; set; }
        public DbSet<CourseEnrollment> CourseEnrollments { get; set; }
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<LessonCompletion> LessonCompletions { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizQuestionOption> QuizQuestionOptions { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CourseEnrollment>()
                .HasIndex(e => new { e.UserId, e.CourseId })
                .IsUnique();

            modelBuilder.Entity<CourseReview>()
                .HasIndex(r => new { r.UserId, r.CourseId })
                .IsUnique();

            modelBuilder.Entity<LessonCompletion>()
                .HasIndex(c => new { c.UserId, c.LessonId })
                .IsUnique()
                .HasFilter("[UserId] IS NOT NULL AND [LessonId] IS NOT NULL");

            modelBuilder.Entity<QuizAttempt>()
                .HasIndex(a => new { a.UserId, a.LessonId, a.AttemptNumber })
                .IsUnique();

            modelBuilder.Entity<QuizAttempt>()
                .HasOne(a => a.Lesson)
                .WithMany(l => l.QuizAttempts)
                .HasForeignKey(a => a.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizAttemptAnswer>()
                .HasOne(a => a.QuizAttempt)
                .WithMany(a => a.Answers)
                .HasForeignKey(a => a.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizAttemptAnswer>()
                .HasOne(a => a.QuizQuestion)
                .WithMany()
                .HasForeignKey(a => a.QuizQuestionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
