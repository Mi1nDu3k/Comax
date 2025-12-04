using System;

namespace Comax.Common.DTOs.Notification
{
    public class NotificationDTO : BaseDto
    {
        public string Message { get; set; }
        public string Url { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}