using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Comax.Data.Repositories
{
    public class ChapterRepository : BaseRepository<Chapter>, IChapterRepository
    {
        public ChapterRepository(ComaxDbContext context) : base(context) { }

        
        public async Task<IEnumerable<Chapter>> GetByComicIdAsync(int comicId)
        {
            return await _dbSet
                .Where(x => x.ComicId == comicId)
                .OrderBy(x => x.Order)
                .ToListAsync();
        }


        public async Task<Chapter?> GetByComicIdAndSlugAsync(int comicId, string chapterSlug)
        {
            return await _context.Chapters
                .Include(c => c.Comic)
                .Include(c => c.Pages.OrderBy(p => p.Index))
                .FirstOrDefaultAsync(c => c.ComicId == comicId && c.Slug == chapterSlug && !c.IsDeleted);
        }


        public override async Task<Chapter?> GetByIdAsync(int id) 
        {
            return await _dbSet
                .Include(c => c.Pages.OrderBy(p => p.Index))
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }
    }
}