using Comax.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, int page, int pageSize);

        // Chỉ để 3 tham số thôi
        Task CreateAndSendNotificationAsync(int userId,string message, string url);

        Task SendNotificationToGroupAsync(List<int> userIds, string message, string url);
        Task MarkAsReadAsync(int id);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteAsync(int id);
    }
}