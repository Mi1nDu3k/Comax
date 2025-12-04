using Comax.Common.DTOs.Notification;
using Comax.Common.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface INotificationService
{
    Task<List<NotificationDTO>> GetMyNotificationsAsync(int userId);
    Task MarkAsReadAsync(int id);
    Task MarkAllAsReadAsync(int userId);
    Task CreateAsync(int userId, string message, string url, NotificationType type);
}