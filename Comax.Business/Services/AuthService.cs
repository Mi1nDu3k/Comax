using Comax.Common.DTOs;
using Comax.Common.DTOs.Auth;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IJwtHelper _jwtHelper;

        public AuthService(IUserRepository userRepo, IJwtHelper jwtHelper)
        {
            _userRepo = userRepo;
            _jwtHelper = jwtHelper;
        }

        public async Task<AuthResultDTO> LoginAsync(LoginDTO dto)
        {
            // 1. Lấy user theo email
            var user = await _userRepo.GetByEmailAsync(dto.Email);

            if (user == null)
            {
                return new AuthResultDTO
                {
                    Success = false,
                    Message = "Email does not exist."
                };
            }

            // 2. Kiểm tra password
            if (!PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return new AuthResultDTO
                {
                    Success = false,
                    Message = "The password is incorrect."
                };
            }

            // 3. Tạo token JWT
            var token = _jwtHelper.GenerateToken(user.Id.ToString(), user.Role?.Name);

            // 4. Trả về kết quả thành công
            return new AuthResultDTO
            {
                Success = true,
                Message = "Login successful!",
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role?.Name
            };
        }
    }
}
