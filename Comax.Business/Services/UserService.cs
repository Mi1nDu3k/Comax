using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Common.DTOs;
using Comax.Common.DTOs.User;
using Comax.Common.Enums;
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
        private readonly INotificationService _notiService;

        public UserService(
            IUserRepository userRepo,
            IRoleRepository roleRepo,
            IMapper mapper,
            IJwtHelper jwtHelper,
            IUnitOfWork unitOfWork,
            INotificationService notificationService) : base(userRepo, unitOfWork, mapper) 
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _mapper = mapper;
            _jwtHelper = jwtHelper;
            _notiService = notificationService;
        }

        #region Authentication

        public async Task<AuthResultDTO> RegisterAsync(RegisterDTO dto)
        {
           
            var existingUser = await _userRepo.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return new AuthResultDTO { Success = false, Message = ErrorMessages.Auth.EmailExists };
            }

            
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

            // 3. Tạo Entity User
            var newUser = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = PasswordHelper.HashPassword(dto.Password),
                RoleId = roleIdToUse,
                IsDeleted = false,
                IsBanned = false, 
                IsVip = false,    
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(newUser);
            await _unitOfWork.CommitAsync(); 

            // 4. Tạo Token & Trả về kết quả
            var token = _jwtHelper.GenerateToken(newUser.Id.ToString(), roleName);

            return new AuthResultDTO
            {
                Success = true,
                Message = ErrorMessages.Auth.RegisterSuccess,
                Token = token,
                Username = newUser.Username,
                UserId = newUser.Id,
                Email = newUser.Email,
                Role = roleName,
                CreatedAt = newUser.CreatedAt,
                RowVersion = newUser.RowVersion
            };
        }

        public async Task<AuthResultDTO> LoginAsync(LoginDTO dto)
        {
           
            var user = await _userRepo.GetByEmailAsync(dto.Email);

           
            if (user == null || !PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return new AuthResultDTO { Success = false, Message = ErrorMessages.Auth.InvalidCredentials };
            }

            
            if (user.IsDeleted)
            {
                return new AuthResultDTO { Success = false, Message = ErrorMessages.Auth.AccountLocked };
            }

            
            if (user.IsBanned)
            {
                return new AuthResultDTO { Success = false, Message = ErrorMessages.Auth.AccountLocked };
            }

            
            string roleName = user.Role?.Name;
            if (string.IsNullOrEmpty(roleName))
            {
                var role = await _roleRepo.GetByIdAsync(user.RoleId);
                roleName = role?.Name ?? "User";
            }

            
            var token = _jwtHelper.GenerateToken(user.Id.ToString(), roleName);
            
            await _notiService.CreateAsync(
            user.Id,
            $"Phát hiện đăng nhập mới vào lúc {DateTime.UtcNow.ToString("HH:mm dd/MM")}",
            "/profile",
            NotificationType.System
        );

            return new AuthResultDTO
            {
                Success = true,
                Message = ErrorMessages.Auth.LoginSuccess,
                Token = token,
                UserId= user.Id,
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
            if (vipRole == null) throw new Exception(ErrorMessages.System.UserNotFound);

            // Cập nhật lên VIP
            user.RoleId = vipRole.Id;
            user.IsVip = true;

            await _userRepo.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            await _notiService.CreateAsync(
            userId,
            ErrorMessages.User.UpgradeSuccess,
            "/profile",
            NotificationType.Account
        );
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
            await _unitOfWork.CommitAsync(); // LƯU VÀO DB
            return true;
        }

        public async Task<List<UserDTO>> GetVipUsersAsync()
        {
            var vipRole = await _roleRepo.GetByNameAsync("VipUser");
            if (vipRole == null) return new List<UserDTO>();

            var users = await _userRepo.GetByRoleIdAsync(vipRole.Id);
            return _mapper.Map<List<UserDTO>>(users);
        }

        #endregion

        #region Moderation (Ban/Unban)

        public async Task<bool> BanUserAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsBanned = true;

            await _userRepo.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            await _notiService.CreateAsync(
            userId,
            ErrorMessages.User.BannedUser,
            "#",
            NotificationType.Account
        );
            return true;
        }

        public async Task<bool> UnbanUserAsync(int userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsBanned = false;

            await _userRepo.UpdateAsync(user);
            await _unitOfWork.CommitAsync(); // LƯU VÀO DB
            return true;
        }

        #endregion
    }
}