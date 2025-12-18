using Microsoft.AspNetCore.Http;

namespace Comax.Common.DTOs.User
{
    public class UserDTO:BaseDto
    {
        public string Username { get; set; }
        public string Email { get; set; }

        public string? Avatar { get; set; }
        public string RoleName { get; set; }
        public bool Isvip { get; set; }
        public bool IsBanned { get; set; }

    }
}
