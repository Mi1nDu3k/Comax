using Comax.Business.Services;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Auth;
using Comax.Common.Helpers;
using Comax.Data.Repositories;
using Comax.Data.Repositories.Interfaces;
using Comax.Shared;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;
namespace Comax.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo; 
        private readonly IJwtHelper _jwtHelper;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        public AuthService(IUserRepository userRepo, IRoleRepository roleRepo, IJwtHelper jwtHelper, IEmailService emailService, IUnitOfWork unitOfWork, IConfiguration config)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _jwtHelper = jwtHelper;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _config = config;
        }

        public async Task<string> LoginAsync(LoginDTO dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);

            if (user == null || !PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return null;
            }

            if (user.IsDeleted || user.IsBanned)
            {
                return null; 
            }

            
            string roleName = user.Role?.Name;
            if (string.IsNullOrEmpty(roleName))
            {
                var role = await _roleRepo.GetByIdAsync(user.RoleId);
                roleName = role?.Name ?? "User";
            }

            return _jwtHelper.GenerateToken(user.Id.ToString(), roleName);
        }


        public async Task<bool> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);
            if (user == null) return false;


            if (user.ResetToken != dto.Otp || user.ResetTokenExpires < DateTime.UtcNow)
            {
                return false;
            }


            user.PasswordHash = PasswordHelper.HashPassword(dto.NewPassword);

            user.ResetToken = null;
            user.ResetTokenExpires = null;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null) return false;
            string otp = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            user.ResetToken = otp;
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(15); // Hết hạn sau 15p

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();


            string frontendUrl = _config["FrontendUrl"] ?? "http://localhost:3000";

            string verifyLink = $"{frontendUrl}/verify-otp?email={email}";

            string userName = !string.IsNullOrEmpty(user.Username) ? user.Username : "Người dùng";
            string emailBody = GetOtpTemplate(userName, otp, verifyLink);

            await _emailService.SendEmailAsync(user.Email, "[Comax] Mã xác thực của bạn", emailBody);

            return true;
        }
        public async Task<bool> VerifyOtpAsync(string email, string otp)
        {
            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null) return false;

            // Kiểm tra khớp mã và còn hạn
            if (user.ResetToken == otp && user.ResetTokenExpires > DateTime.UtcNow)
            {
                return true;
            }
            return false;
        }
        private string GetOtpTemplate(string userName, string otp, string verifyLink)
        {
            string brandColor = "#4F46E5";
            return $@"
    <!DOCTYPE html>
    <html>
    <head><meta charset='UTF-8'></head>
    <body style='font-family: Arial, sans-serif; background-color: #f4f4f5; margin: 0; padding: 0;'>
        <div style='max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1);'>
            <div style='background-color: {brandColor}; padding: 30px; text-align: center;'>
                <h1 style='color: #ffffff; margin: 0; font-size: 24px;'>COMAX</h1>
            </div>
            <div style='padding: 40px 30px; text-align: center;'>
                <h2 style='color: #333333;'>Mã xác thực của bạn</h2>
                <p style='color: #555555;'>Xin chào <strong>{userName}</strong>, sử dụng mã bên dưới để đặt lại mật khẩu:</p>
                
                <div style='background-color: #f0fdf4; border: 1px dashed #16a34a; color: #166534; font-size: 32px; font-weight: bold; letter-spacing: 5px; padding: 15px; margin: 20px 0; border-radius: 8px;'>
                    {otp}
                </div>

                <p style='color: #555555;'>Mã này sẽ hết hạn sau <strong>15 phút</strong>.</p>
                
                <div style='margin-top: 30px;'>
                    <a href='{verifyLink}' style='background-color: {brandColor}; color: #ffffff; text-decoration: none; padding: 10px 20px; border-radius: 5px; font-size: 14px;'>
                        Nhập mã tại Website
                    </a>
                </div>
            </div>
        </div>
    </body>
    </html>";
        }
    }
}