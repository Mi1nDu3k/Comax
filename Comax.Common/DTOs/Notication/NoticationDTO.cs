using System;

namespace Comax.Common.DTOs.Notification
{
    public class NotificationDTO
    {
        public int Id { get; set; }          // Cần để đánh dấu đã đọc
        public string Message { get; set; }  // Nội dung hiển thị
        public string Url { get; set; }      // Cần để click chuyển trang
        public bool IsRead { get; set; }     // Trạng thái đã đọc chưa
        public DateTime CreatedAt { get; set; } // Thời gian


        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
        public string Type { get; set; }     // System, Comment, etc.
    }
}