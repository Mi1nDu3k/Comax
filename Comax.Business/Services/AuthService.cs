using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Auth;
using Comax.Data.Repositories.Interfaces;
using Comax.Common.Helpers;
using Comax.Shared;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo; // Nếu cần lấy role name
        private readonly IJwtHelper _jwtHelper;

        public AuthService(IUserRepository userRepo, IRoleRepository roleRepo, IJwtHelper jwtHelper)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _jwtHelper = jwtHelper;
        }

        public async Task<string> LoginAsync(LoginDTO dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);

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
                var role = await _roleRepo.GetByIdAsync(user.RoleId);
                roleName = role?.Name ?? "User";
            }

            // Tạo Token
            return _jwtHelper.GenerateToken(user.Id.ToString(), roleName);
        }
    }
}