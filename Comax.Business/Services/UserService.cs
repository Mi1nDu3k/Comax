using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Common.DTOs;
using Comax.Common.DTOs.User;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Comax.Shared; // Import để dùng ErrorMessages
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

        #region Authentication

        public async Task<AuthResultDTO> RegisterAsync(RegisterDTO dto)
        {   ///<summary>
            /// Kiểm tra Email trùng
            /// </summary>
            var existingUser = await _userRepo.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return new AuthResultDTO { Success = false, Message = ErrorMessages.Auth.EmailExists };
            }
            ///<summary>
            /// Tìm Role (Mặc định là User nếu không truyền hoặc truyền 0)
            ///</summary>
            int roleIdToUse = dto.RoleId;
            string roleName = "User";

            if (roleIdToUse == 0)
            {
                var defaultRole = await _roleRepo.GetByNameAsync("User");
                if (defaultRole == null)
                    return new AuthResultDTO { Success = false, Message = ErrorMessages.System.DefaultRoleNotFound };

                roleIdToUse = defaultRole.Id;
            }
            else
            {
                var r = await _roleRepo.GetByIdAsync(dto.RoleId);
                if (r != null) roleName = r.Name;
            }
            ///<summary>
            /// 3. Tạo Entity User
            /// </summary>
            var newUser = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                RoleId = roleIdToUse,
                IsDeleted = false,
                IsBanned = false, // Mặc định không bị ban
                IsVip = false,    // Mặc định không phải VIP
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(newUser);
            ///<summary>
            /// Tạo Token & Trả về kết quả
            /// </summary>
            var token = _jwtHelper.GenerateToken(newUser.Id.ToString(), roleName);

            return new AuthResultDTO
            {
                Success = true,
                Message = ErrorMessages.Auth.RegisterSuccess,
                Token = token,
                Username = newUser.Username,
                Email = newUser.Email,
                Role = roleName,
                CreatedAt = newUser.CreatedAt,
                RowVersion = newUser.RowVersion
            };
        }

        public async Task<AuthResultDTO> LoginAsync(LoginDTO dto)
        {
            // 1. Lấy thông tin User
            var user = await _userRepo.GetByEmailAsync(dto.Email);

            // 2. Kiểm tra tài khoản và mật khẩu
            if (user == null || !PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return new AuthResultDTO { Success = false, Message = ErrorMessages.Auth.InvalidCredentials };
            }

            // 3. Kiểm tra xem tài khoản có bị Soft Delete không
            if (user.IsDeleted)
            {
                return new AuthResultDTO { Success = false, Message = ErrorMessages.Auth.AccountLocked };
            }

            // 4. Kiểm tra xem tài khoản có bị BAN không (Logic mới)
            if (user.IsBanned)
            {
                return new AuthResultDTO { Success = false, Message = ErrorMessages.Auth.AccountLocked };
            }

            // 5. Lấy tên Role
            string roleName = user.Role?.Name;
            if (string.IsNullOrEmpty(roleName))
            {
                var role = await _roleRepo.GetByIdAsync(user.RoleId);
                roleName = role?.Name ?? "User";
            }

            // 6. Tạo Token
            var token = _jwtHelper.GenerateToken(user.Id.ToString(), roleName);

            return new AuthResultDTO
            {
                Success = true,
                Message = ErrorMessages.Auth.LoginSuccess,
                Token = token,
                Username = user.Username,
                Email = user.Email,
                Role = roleName
            };
        }

        #endregion

        #region VIP Features

        public async Task<bool> UpgradeToVipAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;

            var vipRole = await _roleRepo.GetByNameAsync("VipUser");
            if (vipRole == null) throw new Exception(ErrorMessages.System.RoleNotFound);

            // Cập nhật lên VIP
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
            if (userRole == null) throw new Exception(ErrorMessages.System.RoleNotFound);

            // Cập nhật về thường
            user.RoleId = userRole.Id;
            user.IsVip = false;

            await _userRepo.UpdateAsync(user);
            return true;
        }

        public async Task<List<UserDTO>> GetVipUsersAsync()
        {
            var vipRole = await _roleRepo.GetByNameAsync("VipUser");
            if (vipRole == null) return new List<UserDTO>(); // Trả về list rỗng nếu chưa có role VIP

            // Giả sử UserRepository có hàm lấy theo RoleId (đã thêm ở các bước trước)
            var users = await _userRepo.GetByRoleIdAsync(vipRole.Id);

            return _mapper.Map<List<UserDTO>>(users);
        }

        #endregion

        #region Moderation (Ban/Unban)

        public async Task<bool> BanUserAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;

            // Set cờ IsBanned
            user.IsBanned = true;

            await _userRepo.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UnbanUserAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;

            // Bỏ cờ IsBanned
            user.IsBanned = false;

            await _userRepo.UpdateAsync(user);
            return true;
        }

        #endregion
    }
}