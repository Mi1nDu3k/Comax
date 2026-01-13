using AutoMapper;
using Comax.Common.DTOs.Comic;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        private readonly IComicRepository _comicRepo;
        private readonly IMapper _mapper;
        public CategoryRepository(ComaxDbContext context) : base(context) { }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
        }
        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted);
        }
        public async Task<List<ComicDTO>> GetFilteredComics(List<int> categoryIds)
        {
            var query = _context.Comics.AsQueryable();

            if (categoryIds != null && categoryIds.Any())
            {
                // Kỹ thuật lọc "AND": 
                // Duyệt qua từng CategoryId yêu cầu, truyện phải thỏa mãn từng cái một
                foreach (var id in categoryIds)
                {
                    query = query.Where(c => c.ComicCategories.Any(cc => cc.CategoryId == id));
                }
            }

            var result = await query
                .Include(c => c.ComicCategories)
                .ToListAsync();

            return _mapper.Map<List<ComicDTO>>(result);
        }
    }
}
