using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comax.Data.Repositories
{
    public class HistoryRepository : BaseRepository<History>, IHistoryRepository
    {
        
        public HistoryRepository(ComaxDbContext context) : base(context)
        {
        }

        public async Task<History?> GetByUserAndComicAsync(int userId, int comicId)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.UserId == userId && x.ComicId == comicId);
        }

        public async Task<IEnumerable<History>> GetByUserAsync(int userId)
        {
            return await _dbSet
                .Include(h => h.Comic)
                .Include(h => h.Chapter)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.LastReadTime)
                .ToListAsync();
        }

       
        public void ForceUpdate(History history)
        {
            _context.Entry(history).State = EntityState.Modified;
        }
    }
}