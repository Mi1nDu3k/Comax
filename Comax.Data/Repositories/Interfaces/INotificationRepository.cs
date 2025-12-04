using Comax.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Data.Repositories.Interfaces
{
    public interface INotificationRepository : IBaseRepository<Notification>
    {
        Task<List<Notification>> GetByUserAsync(int userId);
        Task MarkAllAsReadAsync(int userId);
    }
}