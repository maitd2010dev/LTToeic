using CoreLTToeic.Application.Business;
using CoreLTToeic.Application.Interfaces.IService;
using CoreLTToeic.Infrastructure.Data.Seeders;
using Microsoft.Extensions.DependencyInjection;

namespace CoreLTToeic.Infrastructure.Helper
{
    public static class BuildServices
    {
        public static void AddService(this IServiceCollection services)
        {
            services.AddScoped<ITestService, TestService>();
            services.AddScoped<IUserResultService, UserResultService>();
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ICourseLearningService, CourseLearningService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ToeicTestSeeder>();
            services.AddScoped<ListeningTestSeeder>();
            services.AddScoped<ScoreConversionSeeder>();
            services.AddScoped<AdminSeeder>();
            services.AddScoped<CourseToeicSeeder>();
        }
    }
}
