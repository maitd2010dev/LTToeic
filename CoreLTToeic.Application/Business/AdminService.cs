using CoreLTToeic.Application.Interfaces.IRepository;
using CoreLTToeic.Application.Interfaces.IService;
using CoreLTToeic.Application.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CoreLTToeic.Application.Business;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepo;

    public AdminService(IAdminRepository adminRepo)
    {
        _adminRepo = adminRepo;
    }

    public Task<AdminDashboardViewModel> GetDashboardStatsAsync()
        => _adminRepo.GetDashboardStatsAsync();

    public Task<List<UserResultViewModel>> GetAllResultsAsync()
        => _adminRepo.GetAllResultsAsync();

    public Task<List<UserManagementViewModel>> GetAllUsersAsync()
        => _adminRepo.GetAllUsersAsync();

    public Task<IdentityResult> LockUserAsync(string userId)
        => _adminRepo.LockUserAsync(userId);

    public Task<IdentityResult> UnlockUserAsync(string userId)
        => _adminRepo.UnlockUserAsync(userId);
}
