using AutoMapper;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Auth;
using Comax.Common.DTOs.User;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Comax.Shared;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Comax.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDTO> GetByEmailAsync(string email)
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var user = users.FirstOrDefault(x => x.Email == email);
            return _mapper.Map<UserDTO>(user);
        }

        public async Task<ServiceResponse<UserDTO>> RegisterAsync(RegisterDTO registerDto)
        {
            var existingUser = await GetByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return ServiceResponse<UserDTO>.Error("Email đã tồn tại");
            }

            var user = _mapper.Map<User>(registerDto);
            user.PasswordHash = registerDto.Password;

            // 👇 SỬA QUAN TRỌNG: Gán RoleId (int) thay vì Role (Entity)
            // Ép kiểu Enum sang int (Ví dụ: User = 1)
            // Lưu ý: Đảm bảo ID trong Database khớp với số thứ tự Enum
            user.RoleId = (int)Comax.Common.Enums.Role.User;

            user.IsVip = false;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CommitAsync();

            var userDto = _mapper.Map<UserDTO>(user);
            return ServiceResponse<UserDTO>.Ok(userDto, "Đăng ký thành công");
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

            // 👇 SỬA: Thêm await
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DowngradeFromVipAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsVip = false;

            // 👇 SỬA: Thêm await
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<List<UserDTO>> GetVipUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            var vips = users.Where(u => u.IsVip == true);
            return _mapper.Map<List<UserDTO>>(vips);
        }

        public async Task<bool> BanUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            // user.IsBanned = true; // Mở comment này nếu bạn đã có trường IsBanned trong Entity

            // 👇 SỬA: Thêm await
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> UnbanUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            // user.IsBanned = false; 

            // 👇 SỬA: Thêm await
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id, bool hardDelete)
        {
            return await _unitOfWork.Users.DeleteAsync(id, hardDelete);
        }
    }
}