using Comax.Common.Enums; 

namespace Comax.Data.Entities
{
    public class Notification : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public string Message { get; set; }
        public string Url { get; set; }
        public bool IsRead { get; set; } = false;
        public NotificationType Type { get; set; } = NotificationType.System;
    }
}