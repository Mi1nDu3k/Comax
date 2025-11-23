using Comax.Common.DTOs;

namespace Comax.Common.Helpers
{
    public interface IJwtHelper
    {
        string GenerateToken(string userId, string role);
    }
}
