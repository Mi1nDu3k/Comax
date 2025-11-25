namespace Comax.Data.Entities
{
    // Thêm kế thừa BaseEntity
    public class User : BaseEntity
    {
        // Xóa public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}