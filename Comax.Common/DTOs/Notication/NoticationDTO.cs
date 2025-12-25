using System;

namespace Comax.Common.DTOs.Notification
{
    public class NotificationDTO : BaseDto
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Type { get; set; }
    }
}