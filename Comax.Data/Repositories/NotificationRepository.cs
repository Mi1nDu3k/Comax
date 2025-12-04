using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comax.Data.Repositories
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ComaxDbContext context) : base(context) { }

        public async Task<List<Notification>> GetByUserAsync(int userId)
        {
            return await _dbSet
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(20)
                .ToListAsync();
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unread = await _dbSet.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            foreach (var n in unread) n.IsRead = true;
        }
    }
}