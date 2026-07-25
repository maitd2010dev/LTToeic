using CoreLTToeic.Application.Interfaces.IRepository;
using CoreLTToeic.Application.Interfaces;
using CoreLTToeic.Domain.Entities;
using CoreLTToeic.Infrastructure.Context;
using CoreLTToeic.Infrastructure.Pattern;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using CoreLTToeic.Application.Models.EditModels;
using CoreLTToeic.Application.Common.Constants;
using Microsoft.Data.SqlClient;

namespace CoreLTToeic.Infrastructure.Repositories
{
    public class AuthRepository : Repository<AppUser>, IAuthRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AuthRepository
            (
            IDbContextFactory<AppDbContext> factory,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IConfiguration configuration
            )
            : base(factory)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public async Task<IdentityResult> Login(LoginEditModel loginModel)
        {
            var currentUser = await _userManager.FindByNameAsync(loginModel.UserName);
            if (currentUser == null)
            {
                throw new Exception(MessageConstants.USER_NOT_EXIST);
            }

            if (_userManager.Options.SignIn.RequireConfirmedEmail && !await _userManager.IsEmailConfirmedAsync(currentUser))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email chưa được xác nhận, vui lòng xác nhận lại trong email" });
            }

            if (await _userManager.IsLockedOutAsync(currentUser))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên." });
            }

            if (await _userManager.CheckPasswordAsync(currentUser, loginModel.Password))
            {
                return IdentityResult.Success;
            }

            return IdentityResult.Failed(new IdentityError { Description = "Tài khoản hoặc mật khẩu không đúng" });
        }

        public async Task<IdentityResult> Register(RegisterEditModel registerModel)
        {
            try
            {
                if (await _userManager.FindByNameAsync(registerModel.UserName) != null)
                {
                    throw new Exception(MessageConstants.USER_ALREADY_EXIST);
                }

                var newUser = new AppUser
                {
                    UserName = registerModel.UserName,
                    Email = registerModel.Email,
                    FullName = registerModel.FullName,
                    CreateTime = DateTime.UtcNow,
                };

                IdentityResult res;
                try
                {
                    res = await _userManager.CreateAsync(newUser, registerModel.Password);
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
                {
                    throw new Exception("Tên đăng nhập hoặc email đã được sử dụng.");
                }

                if (res.Succeeded)
                {
                    try
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                        var encodedToken = WebUtility.UrlEncode(token);

                        var baseUrl = _configuration["App:BaseUrl"]?.TrimEnd('/') ?? string.Empty;
                        var confirmPath = _configuration["App:ConfirmEmailPath"] ?? "/api/auth/confirmemail";
                        var confirmUrl = $"{baseUrl}{confirmPath}?userId={WebUtility.UrlEncode(newUser.Id)}&token={encodedToken}";

                        var subject = "Confirm your LTToeic account";
                        var body = $@"
                        <p>Hi {newUser.FullName},</p>
                        <p>Please confirm your account by clicking the link below:</p>
                        <p><a href=""{confirmUrl}"">Confirm email</a></p>
                        <p>If you did not request this, ignore this email.</p>
                        <p>Regards,<br/>LTToeic Team</p>";

                        await _emailSender.SendEmailAsync(newUser.Email, subject, body);
                    }
                    catch
                    {
                        // Do not fail registration if email sending fails. Log the error (add ILogger) if desired.
                    }
                }

                return res;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result;
        }

        public async Task<AppUser?> GetByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityResult> UpdateProfileAsync(string userId, UpdateProfileEditModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng" });

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.DateOfBirth = model.DateOfBirth;

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Không tìm thấy người dùng" });

            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }
    }
}
