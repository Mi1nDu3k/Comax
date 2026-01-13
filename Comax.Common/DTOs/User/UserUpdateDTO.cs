using Comax.Common.Enums;
using Microsoft.AspNetCore.Http;

namespace Comax.Common.DTOs.User
{
    public class UserUpdateDTO : BaseDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? RoleId { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }
}
