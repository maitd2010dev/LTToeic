using AutoMapper;
using CoreLTToeic.Application.Interfaces.IRepository;
using CoreLTToeic.Application.Models.ViewModels;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Domain.Enums;
using CoreLTToeic.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CoreLTToeic.Infrastructure.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly UserManager<AppUser> _userManager;
    private readonly IMapper _mapper;

    public AdminRepository(
        IDbContextFactory<AppDbContext> factory,
        UserManager<AppUser> userManager,
        IMapper mapper)
    {
        _factory = factory;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
    {
        using var ctx = await _factory.CreateDbContextAsync();
        var cutoff = DateTime.Today.AddDays(-6);
        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var firstMonth = currentMonth.AddMonths(-11);
        var nextMonth = currentMonth.AddMonths(1);

        var totalUsers     = await ctx.AppUser.CountAsync();
        var totalTests     = await ctx.Tests.CountAsync();
        var totalCompleted = await ctx.UserResults.CountAsync(r => r.AttemptStatus == AttemptStatus.Completed);
        var avgScore       = await ctx.UserResults
            .Where(r => r.AttemptStatus == AttemptStatus.Completed)
            .AverageAsync(r => (double?)r.TotalScore);
        var totalCourses   = await ctx.Courses.CountAsync();

        var dailyRaw = await ctx.UserResults
            .Where(r => r.CompletedAt.HasValue && r.CompletedAt.Value.Date >= cutoff)
            .GroupBy(r => r.CompletedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        var monthlyParticipantsRaw = await ctx.UserResults
            .Where(r => r.AttemptStatus == AttemptStatus.Completed
                && r.CompletedAt.HasValue
                && r.CompletedAt.Value >= firstMonth
                && r.CompletedAt.Value < nextMonth)
            .GroupBy(r => new
            {
                r.CompletedAt!.Value.Year,
                r.CompletedAt.Value.Month
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Count = g.Select(r => r.UserId).Distinct().Count()
            })
            .ToListAsync();

        var topTestsRaw = await ctx.Tests
            .Select(t => new { t.Title, Count = t.UserResults.Count })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        var activityDict = dailyRaw.ToDictionary(x => x.Date, x => x.Count);
        var dailySeries = Enumerable.Range(0, 7)
            .Select(i => DateTime.Today.AddDays(i - 6))
            .Select(d => new DailyActivityViewModel
            {
                Date  = d.ToString("dd/MM"),
                Count = activityDict.GetValueOrDefault(d, 0)
            })
            .ToList();

        var monthlyParticipantDict = monthlyParticipantsRaw.ToDictionary(
            x => (x.Year, x.Month),
            x => x.Count);
        var monthlyParticipantSeries = Enumerable.Range(0, 12)
            .Select(i => firstMonth.AddMonths(i))
            .Select(month => new MonthlyExamParticipantViewModel
            {
                Month = month.ToString("MM/yyyy"),
                ParticipantCount = monthlyParticipantDict.GetValueOrDefault(
                    (month.Year, month.Month),
                    0)
            })
            .ToList();

        return new AdminDashboardViewModel
        {
            TotalUsers            = totalUsers,
            TotalTests            = totalTests,
            TotalCompletedResults = totalCompleted,
            AverageTotalScore     = (int)Math.Round(avgScore ?? 0),
            TotalCourses          = totalCourses,
            DailyActivity         = dailySeries,
            MonthlyExamParticipants = monthlyParticipantSeries,
            TopTests              = topTestsRaw
                .Select(t => new TopTestViewModel
                {
                    TestTitle    = t.Title,
                    AttemptCount = t.Count
                })
                .ToList()
        };
    }

    public async Task<List<UserResultViewModel>> GetAllResultsAsync()
    {
        using var ctx = await _factory.CreateDbContextAsync();
        var results = await ctx.UserResults
            .Include(r => r.Test)
            .Include(r => r.User)
            .OrderByDescending(r => r.CompletedAt)
            .ToListAsync();
        return _mapper.Map<List<UserResultViewModel>>(results);
    }

    public async Task<List<UserManagementViewModel>> GetAllUsersAsync()
    {
        using var ctx = await _factory.CreateDbContextAsync();

        var users = await ctx.AppUser
            .OrderByDescending(u => u.CreateTime)
            .ToListAsync();

        var examCounts = await ctx.UserResults
            .GroupBy(r => r.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        var userRolesRaw = await ctx.UserRoles
            .Join(ctx.Roles,
                  ur => ur.RoleId,
                  r  => r.Id,
                  (ur, r) => new { ur.UserId, RoleName = r.Name })
            .ToListAsync();

        var rolesByUser = userRolesRaw
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => g.First().RoleName ?? "User");

        var now = DateTimeOffset.UtcNow;
        return users.Select(u => new UserManagementViewModel
        {
            Id             = u.Id,
            FullName       = u.FullName,
            Email          = u.Email,
            UserName       = u.UserName,
            CreateTime     = u.CreateTime,
            LastLogin      = u.LastLogin,
            EmailConfirmed = u.EmailConfirmed,
            IsLocked       = u.LockoutEnd.HasValue && u.LockoutEnd.Value > now,
            Role           = rolesByUser.GetValueOrDefault(u.Id, "User"),
            ExamCount      = examCounts.GetValueOrDefault(u.Id, 0)
        }).ToList();
    }

    public async Task<IdentityResult> LockUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng" });

        var enableLockoutResult = await _userManager.SetLockoutEnabledAsync(user, true);
        if (!enableLockoutResult.Succeeded)
            return enableLockoutResult;

        var lockResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        if (!lockResult.Succeeded)
            return lockResult;

        return await _userManager.UpdateSecurityStampAsync(user);
    }

    public async Task<IdentityResult> UnlockUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng" });

        return await _userManager.SetLockoutEndDateAsync(user, null);
    }
}
