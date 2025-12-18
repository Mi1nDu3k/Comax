using Comax.Common.DTOs.Notification;
using Comax.Common.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Interfaces 
{
    public interface INotificationService
    {
        Task<List<NotificationDTO>> GetUserNotificationsAsync(int userId);
        Task MarkAsReadAsync(int id);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteAsync(int notificationId, int userId);

        Task CreateAndSendNotificationAsync(int userId, string message, string url, NotificationType type = NotificationType.System);
    }
}