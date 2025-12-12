using Comax.Common.DTOs.Auth;
using System.Threading.Tasks;

namespace Comax.Business.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginDTO loginDto);
    }
}