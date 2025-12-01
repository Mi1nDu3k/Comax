using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs;
using Comax.Common.DTOs.User;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class UserService : BaseService<User, UserDTO, UserCreateDTO, UserUpdateDTO>, IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IJwtHelper _jwtHelper;
        private readonly IMapper _mapper;

        public UserService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IMapper mapper,
            IJwtHelper jwtHelper) : base(userRepo, mapper)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
        }

        public async Task<AuthResultDTO> RegisterAsync(RegisterDTO dto)
        {
            // 1. Kiểm tra Email trùng
            var existingUser = await _userRepo.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return new AuthResultDTO { Success = false, Message = "Email đã tồn tại." };
            }

            // 2. Tìm Role
            int roleIdToUse = dto.RoleId;
            string roleName = "User";

            if (roleIdToUse == 0)
            {
                var defaultRole = await _roleRepo.GetByNameAsync("User");
                if (defaultRole == null)
                    return new AuthResultDTO { Success = false, Message = "Lỗi hệ thống: Không tìm thấy Role mặc định." };

                roleIdToUse = defaultRole.Id;
            }
            else
            {
                var r = await _roleRepo.GetByIdAsync(dto.RoleId);
                if (r != null) roleName = r.Name;
            }

            // 3. Tạo Entity
            var newUser = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                RoleId = roleIdToUse,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                IsVip = false
            };

            await _userRepo.AddAsync(newUser);
            // await _userRepo.SaveChangesAsync(); // Bỏ comment nếu BaseRepo chưa save

            // 4. Tạo Token (SỬA LỖI TẠI ĐÂY: Truyền Id.ToString() thay vì object User)
            var token = _jwtHelper.GenerateToken(newUser.Id.ToString(), roleName);

            return new AuthResultDTO
            {
                Success = true,
                Message = "Đăng ký thành công!",
                Token = token,
                Username = newUser.Username,
                Email = newUser.Email,
                Role = roleName
            };
        }

        public async Task<AuthResultDTO> LoginAsync(LoginDTO dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);

            if (user == null || !PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return new AuthResultDTO { Success = false, Message = "Tài khoản hoặc mật khẩu không đúng." };
            }

            if (user.IsDeleted)
                return new AuthResultDTO { Success = false, Message = "Tài khoản đã bị khóa." };

            string roleName = user.Role?.Name;
            if (string.IsNullOrEmpty(roleName))
            {
                var role = await _roleRepo.GetByIdAsync(user.RoleId);
                roleName = role?.Name ?? "User";
            }

            // (SỬA LỖI TẠI ĐÂY: Truyền Id.ToString())
            var token = _jwtHelper.GenerateToken(user.Id.ToString(), roleName);

            return new AuthResultDTO
            {
                Success = true,
                Message = "Đăng nhập thành công!",
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = roleName
            };
        }

        // ... Các hàm UpgradeToVipAsync, DowngradeFromVipAsync, GetVipUsersAsync giữ nguyên ...
        public async Task<bool> UpgradeToVipAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;
            var vipRole = await _roleRepo.GetByNameAsync("VipUser");
            if (vipRole == null) return false;
            user.RoleId = vipRole.Id;
            user.IsVip = true;
            await _userRepo.UpdateAsync(user);
            return true;
        }

        public async Task<bool> DowngradeFromVipAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;
            var userRole = await _roleRepo.GetByNameAsync("User");
            if (userRole == null) return false;
            user.RoleId = userRole.Id;
            user.IsVip = false;
            await _userRepo.UpdateAsync(user);
            return true;
        }

        public async Task<List<UserDTO>> GetVipUsersAsync()
        {
            var vipRole = await _roleRepo.GetByNameAsync("VipUser");
            if (vipRole == null) return new List<UserDTO>();
            var users = await _userRepo.GetByRoleIdAsync(vipRole.Id);
            return _mapper.Map<List<UserDTO>>(users);
        }
    }
}