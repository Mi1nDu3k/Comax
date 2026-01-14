using AutoMapper;
using Comax.Common.DTOs.Comic;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        // 1. Chỉ giữ lại những gì cần thiết
        private readonly IMapper _mapper;

        // 2. SỬA LỖI: Phải Inject mapper vào Constructor
        public CategoryRepository(ComaxDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug && !c.IsDeleted);
        }

        // 3. SỬA LỖI LOGIC LỌC
        public async Task<List<ComicDTO>> GetFilteredComics(List<int> categoryIds)
        {
            var query = _context.Comics.AsQueryable();

            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(c => c.ComicCategories
                                 .Any(cc => categoryIds.Contains(cc.CategoryId)));
            }

            // 4. Quan trọng: Thêm Distinct để tránh 1 truyện hiện 2 lần nếu nó thuộc cả 2 category
            var result = await query
                .Include(c => c.ComicCategories)
                .Distinct()
                .ToListAsync();

            return _mapper.Map<List<ComicDTO>>(result);
        }
    }
}