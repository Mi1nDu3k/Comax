using Comax.Common.DTOs;
using Comax.Common.DTOs.User;

namespace Comax.Business.Interfaces
{
    public interface IUserService : IBaseService<UserDTO, UserCreateDTO, UserUpdateDTO>
    {
        Task<AuthResultDTO> RegisterAsync(RegisterDTO dto);
        Task<AuthResultDTO> LoginAsync(LoginDTO dto);
    }
}
