using Comax.Common.DTOs.Pagination;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class ComicRepository : BaseRepository<Comic>, IComicRepository
    {
        public ComicRepository(ComaxDbContext context) : base(context) { }



        public async Task<Comic?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Author)    
                .Include(c => c.ComicCategories)
                        .ThenInclude(cc => cc.Category)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

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
        public async Task<PagedList<Comic>> GetLatestUpdatedComicsAsync(PaginationParams param)
        {
            var query = _context.Comics
                .Include(c => c.Author)
                .Include(c => c.Chapters)
                .Include(c => c.ComicCategories)
                .ThenInclude(cc => cc.Category)
                .Where(c => !c.IsDeleted)
                
                .OrderByDescending(c => c.Chapters.Max(ch => (DateTime?)ch.PublishDate) ?? c.CreatedAt);

            var count = await query.CountAsync();
            var items = await query
                .Skip((param.PageNumber - 1) * param.PageSize)
                .Take(param.PageSize)
                .ToListAsync();

            return new PagedList<Comic>(items, count, param.PageNumber, param.PageSize);
        }
        public async Task<Comic?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Comics
                .Include(c => c.Author)
                .Include(c => c.ComicCategories)
                    .ThenInclude(cc => cc.Category) // Lấy thông tin Category để lấy Name
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }
    }
}
