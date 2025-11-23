using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class ChapterRepository : BaseRepository<Chapter>, IChapterRepository
    {
        public ChapterRepository(ComaxDbContext context) : base(context) { }

        public async Task<IEnumerable<Chapter>> GetByComicIdAsync(int comicId)
        {
            return await _dbSet
                .Where(ch => ch.ComicId == comicId)
                .OrderBy(ch => ch.Order)
                .ToListAsync();
        }
    }
}
