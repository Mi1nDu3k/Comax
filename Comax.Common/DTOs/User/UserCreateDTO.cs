using Comax.Common.Enums;

namespace Comax.Common.DTOs.User
{
    public class UserCreateDTO: BaseDto 
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
    }
}
