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
                .Where(x => x.ComicId == comicId)
                .OrderBy(x => x.Order) // Sắp xếp luôn nếu muốn
                .ToListAsync();
        }

 
        public async Task<Chapter?> GetByComicIdAndSlugAsync(int comicId, string chapterSlug)
        {
            return await _context.Chapters
                .Include(c => c.Comic) 
                .FirstOrDefaultAsync(c => c.ComicId == comicId && c.Slug == chapterSlug && !c.IsDeleted);
        }
    }
}
 