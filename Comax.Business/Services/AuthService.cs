using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Comax.Business.Services.Interfaces;
using Comax.Business.Interfaces; // Để dùng IEmailService
using Comax.Common.DTOs.Auth;
using Comax.Common.Helpers;
using Comax.Data.Repositories.Interfaces; // Để dùng IUnitOfWork

namespace Comax.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtHelper _jwtHelper;
        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;
        private readonly IEmailService _emailService;

     
        public AuthService(
            IUnitOfWork unitOfWork,
            IJwtHelper jwtHelper,
            IConfiguration config,
            IDistributedCache cache,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _jwtHelper = jwtHelper;
            _config = config;
            _cache = cache;
            _emailService = emailService;
        }

        // --- 1. LOGIN (CODE CŨ ĐÃ ĐƯỢC CHUYỂN SANG DÙNG UNIT OF WORK) ---
        public async Task<string> LoginAsync(LoginDTO dto)
        {
            // Lấy user từ UnitOfWork
            var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);

            // Kiểm tra mật khẩu (Dùng Helper có sẵn của bạn)
            if (user == null || !PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return null; // Login thất bại
            }

            if (user.IsDeleted || user.IsBanned)
            {
                return null; // Tài khoản bị khóa
            }

            // Lấy tên Role
            string roleName = user.Role?.Name;
            if (string.IsNullOrEmpty(roleName))
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId);
                roleName = role?.Name ?? "User";
            }

            // Tạo Token
            return _jwtHelper.GenerateToken(user.Id.ToString(), roleName);
        }

        // --- 2. QUÊN MẬT KHẨU (CODE MỚI) ---
        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null) return; // Không báo lỗi để bảo mật

            // Tạo Token ngẫu nhiên
            string token = Guid.NewGuid().ToString();
            string cacheKey = $"RESET_PASS_{email}";

            // Lưu vào Redis (Token sống 15 phút)
            await _cache.SetStringAsync(cacheKey, token, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            });

            // Lấy URL frontend từ cấu hình (hoặc mặc định localhost)
            string frontendUrl = _config["FrontendUrl"] ?? "http://localhost:3000";

            // Tạo link reset
            string resetLink = $"{frontendUrl}/auth/reset-password?email={email}&token={token}";

            string emailBody = $@"
                <h3>Yêu cầu đặt lại mật khẩu Comax</h3>
                <p>Click vào link dưới đây để đặt lại mật khẩu (Link hết hạn sau 15 phút):</p>
                <a href='{resetLink}' style='padding: 10px 20px; background-color: #facc15; color: black; text-decoration: none; border-radius: 5px;'>Đặt lại mật khẩu</a>
                <p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>";

            // Gửi mail
            await _emailService.SendEmailAsync(email, "Reset Password Comax", emailBody);
        }

        // --- 3. ĐẶT LẠI MẬT KHẨU (CODE MỚI - ĐÃ SỬA LỖI SAVEASYNC) ---
        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            // 1. Kiểm tra Token trong Redis
            string cacheKey = $"RESET_PASS_{email}";
            var storedToken = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(storedToken) || storedToken != token)
            {
                return false; 
            }

            // 2. Lấy User
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null) return false;

            // 3. Cập nhật mật khẩu mới (Dùng Helper hash password cho đồng bộ)
            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
          

            // 4. Lưu xuống DB (Dùng UpdateAsync của Repo User nếu cần, hoặc UnitOfWork tự tracking)
            await _unitOfWork.Users.UpdateAsync(user);

          
            await _unitOfWork.CommitAsync();

            // 5. Xóa Token khỏi Redis để không dùng lại được
            await _cache.RemoveAsync(cacheKey);

            return true;
        }
    }
}