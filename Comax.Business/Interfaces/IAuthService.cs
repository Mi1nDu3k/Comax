using Comax.Common.DTOs.Auth;
using System.Threading.Tasks;

namespace Comax.Business.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(LoginDTO loginDto);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(ResetPasswordDTO dto);
        Task<bool> VerifyOtpAsync(string email, string otp);
    }
}