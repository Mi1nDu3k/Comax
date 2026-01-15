using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.Constants;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Auth;
using Comax.Common.DTOs.User;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Comax.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IStorageService storageService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<UserDTO> GetByEmailAsync(string email)
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var user = users.FirstOrDefault(x => x.Email.ToLower() == email.ToLower()); 

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<ServiceResponse<UserDTO>> RegisterAsync(RegisterDTO registerDto)
        {
            var existingUser = await GetByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return ServiceResponse<UserDTO>.Error(SystemMessages.Auth.EmailExists);
            }

            var user = _mapper.Map<User>(registerDto);
            user.PasswordHash = Comax.Common.Helpers.PasswordHelper.HashPassword(registerDto.Password);
            user.RoleId = (int)Comax.Common.Enums.Role.User;
            user.IsVip = false;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CommitAsync();

            var userDto = _mapper.Map<UserDTO>(user);
            return ServiceResponse<UserDTO>.Ok(userDto, SystemMessages.Auth.RegisterSuccess);
        }

        public async Task<IEnumerable<UserDTO>> GetAllAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<bool> UpgradeToVipAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsVip = true;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DowngradeFromVipAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsVip = false;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<List<UserDTO>> GetVipUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var vips = users.Where(u => u.IsVip == true).ToList();
            return _mapper.Map<List<UserDTO>>(vips);
        }

        public async Task<bool> BanUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> UnbanUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            // Logic Unban
            // user.IsBanned = false;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, bool hardDelete)
        {
            return await _unitOfWork.Users.DeleteAsync(id, hardDelete);
        }

        public async Task<UserDTO> GetByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return null;
            return _mapper.Map<UserDTO>(user);
        }

        // --- HÀM SỬA LỖI 500 ---
        public async Task<ServiceResponse<UserDTO>> UpdateProfileAsync(int userId, UserUpdateDTO request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return ServiceResponse<UserDTO>.Error(SystemMessages.Auth.UserNotFound);

            // 1. Map dữ liệu
            _mapper.Map(request, user);

            // 2. Xử lý Avatar
            if (request.AvatarFile != null)
            {
                // Xóa ảnh cũ (Đã bổ sung logic)
                if (!string.IsNullOrEmpty(user.Avatar) && !user.Avatar.Contains("placehold"))
                {
                    try
                    {
                        // Gọi hàm xóa file (Nếu MinioStorageService có hàm DeleteFileAsync)
                        await _storageService.DeleteFileAsync(user.Avatar);
                    }
                    catch { /* Bỏ qua lỗi xóa file cũ để không chặn luồng */ }
                }

                // Upload ảnh mới
                var avatarUrl = await _storageService.UploadFileAsync(request.AvatarFile, "avatars");
                user.Avatar = avatarUrl;
            }

            // --- QUAN TRỌNG: THÊM AWAIT ---
            await _unitOfWork.Users.UpdateAsync(user);
            // ------------------------------

            await _unitOfWork.CommitAsync();

            return ServiceResponse<UserDTO>.Ok(_mapper.Map<UserDTO>(user));
        }
    }
}