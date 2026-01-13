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

        public async Task<List<Notification>> GetByUserIdAsync(int userId, int page, int pageSize)
        {
            return await _dbSet
                .Where(n => n.UserId == userId)
                .Include(n => n.Sender)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(); 
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _dbSet.Where(n => n.UserId == userId && !n.IsRead)
                        .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }
    }
}