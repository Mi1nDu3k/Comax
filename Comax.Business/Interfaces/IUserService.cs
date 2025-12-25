using Comax.Common.DTOs.Auth; // Thêm dòng này
using Comax.Common.DTOs.User;
using Comax.Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllAsync();

        Task<UserDTO> GetByEmailAsync(string email);
        // Đảm bảo Interface có dòng này
        Task<UserDTO> GetByIdAsync(int id);

        //Task<UserDTO> RegisterAsync(RegisterDTO registerDto);
        Task<ServiceResponse<UserDTO>> RegisterAsync(RegisterDTO registerDto);
        Task<bool> UpgradeToVipAsync(int userId);
        Task<bool> DowngradeFromVipAsync(int userId);
        Task<List<UserDTO>> GetVipUsersAsync();
        Task<bool> BanUserAsync(int userId);
        Task<bool> UnbanUserAsync(int userId);
        Task<bool> DeleteAsync(int id, bool hardDelete);
   
        Task<ServiceResponse<UserDTO>> UpdateProfileAsync(int userId, UserUpdateDTO request);
    }
}