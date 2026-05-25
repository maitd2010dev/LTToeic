using CoreLTToeic.Application.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CoreLTToeic.Application.Interfaces.IRepository;

public interface IAdminRepository
{
    Task<AdminDashboardViewModel> GetDashboardStatsAsync();
    Task<List<UserResultViewModel>> GetAllResultsAsync();
    Task<List<UserManagementViewModel>> GetAllUsersAsync();
    Task<IdentityResult> LockUserAsync(string userId);
    Task<IdentityResult> UnlockUserAsync(string userId);
}
