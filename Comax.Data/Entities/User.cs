using Comax.Common.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comax.Data.Entities
{
    // Thêm kế thừa BaseEntity
    public class User : BaseEntity
    {
        // Xóa public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? Avatar { get; set; }
        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
        public bool IsVip { get; set; } = false;
        public bool IsBanned { get; set; }=false;
        public DateTime? VipExpireAt { get; set; }
        public SubscriptionStatus SubStatus { get; set; } = SubscriptionStatus.Active;
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
    }
}