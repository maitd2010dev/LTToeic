using CoreLTToeic.Application.Business;
using CoreLTToeic.Application.Interfaces;
using CoreLTToeic.Application.Interfaces.IRepository;
using CoreLTToeic.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLTToeic.Infrastructure.Helper
{
    public static class BuildRepositories
    {
        public static void AddRepository(this IServiceCollection services)
        {
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<ITestRepository, TestRepository>();
            services.AddScoped<IQuestionRepository, QuestionRepository>();
            services.AddScoped<ITestCategoryRepository, TestCategoryRepository>();
            services.AddScoped<IUserResultRepository, UserResultRepository>();
            services.AddScoped<IQuestionGroupRepository, QuestionGroupRepository>();
            services.AddScoped<IPartRepository, PartRepository>();
            services.AddScoped<ICourseRepository, CourseRepository>();
            services.AddScoped<IScoreConversionRepository, ScoreConversionRepository>();
            services.AddScoped<IAdminRepository, AdminRepository>();
        }
    }
}
