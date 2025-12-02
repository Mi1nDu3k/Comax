using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class ComicRepository : BaseRepository<Comic>, IComicRepository
    {
        public ComicRepository(ComaxDbContext context) : base(context) { }

        public async Task<Comic?> GetBySlugAsync(string slug)
        {
          

            return await _context.Comics
                .Include(c => c.Author) 
                .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted);
        }

        public async Task<IEnumerable<Comic>> SearchByTitleAsync(string title)
        {
            return await _dbSet
                .Where(c => c.Title.Contains(title))
                .ToListAsync();
        }

        public async Task<IEnumerable<Comic>> GetByAuthorIdAsync(int authorId)
        {
            return await _dbSet
                .Where(c => c.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comic>> GetByCategoryIdAsync(int categoryId)
        {
            return await _dbSet
                .Where(c => c.ComicCategories.Any(cc => cc.CategoryId == categoryId))
                .ToListAsync();
        }
    }
}
