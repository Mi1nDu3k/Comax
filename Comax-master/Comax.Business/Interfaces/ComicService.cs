using Comax.Business.Interfaces;
using Comax.Data;
using Comax.Data.Entities;
using Comax.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Comax.Business.Services
{
    public class ComicService : IComicService
    {
        private readonly IComicRepository _comicRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly ComaxDbContext _context;

        public ComicService(IComicRepository comicRepo, ICategoryRepository categoryRepo, ComaxDbContext context)
        {
            _comicRepo = comicRepo;
            _categoryRepo = categoryRepo;
            _context = context;
        }

        public async Task<IEnumerable<Comic>> GetAllAsync()
        {
            return await _context.Comics
                .Include(c => c.Author)
                .Include(c => c.Chapters)
                .Include(c => c.ComicCategories)
                    .ThenInclude(cc => cc.Category)
                .ToListAsync();
        }

        public async Task<Comic?> GetByIdAsync(int id)
        {
            return await _context.Comics
                .Include(c => c.Author)
                .Include(c => c.Chapters)
                .Include(c => c.ComicCategories)
                    .ThenInclude(cc => cc.Category)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Comic>> SearchByTitleAsync(string title)
        {
            return await _context.Comics
                .Include(c => c.Author)
                .Include(c => c.Chapters)
                .Include(c => c.ComicCategories)
                    .ThenInclude(cc => cc.Category)
                .Where(c => c.Title.Contains(title))
                .ToListAsync();
        }

        public async Task<IEnumerable<Comic>> GetByAuthorIdAsync(int authorId)
        {
            return await _context.Comics
                .Include(c => c.Author)
                .Include(c => c.Chapters)
                .Include(c => c.ComicCategories)
                    .ThenInclude(cc => cc.Category)
                .Where(c => c.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comic>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.ComicCategories
                .Where(cc => cc.CategoryId == categoryId)
                .Select(cc => cc.Comic)
                .Include(c => c.Author)
                .Include(c => c.Chapters)
                .Include(c => c.ComicCategories)
                    .ThenInclude(cc => cc.Category)
                .ToListAsync();
        }

        public async Task AddAsync(Comic comic, List<int>? categoryIds = null)
        {
            _context.Comics.Add(comic);
            await _context.SaveChangesAsync();

            if (categoryIds != null && categoryIds.Any())
            {
                foreach (var catId in categoryIds)
                {
                    _context.ComicCategories.Add(new ComicCategory
                    {
                        ComicId = comic.Id,
                        CategoryId = catId
                    });
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Comic comic, List<int>? categoryIds = null)
        {
            _context.Comics.Update(comic);

            // Update categories
            var existingCategories = _context.ComicCategories.Where(cc => cc.ComicId == comic.Id);
            _context.ComicCategories.RemoveRange(existingCategories);

            if (categoryIds != null && categoryIds.Any())
            {
                foreach (var catId in categoryIds)
                {
                    _context.ComicCategories.Add(new ComicCategory
                    {
                        ComicId = comic.Id,
                        CategoryId = catId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Comic comic)
        {
            // Xóa liên kết ComicCategory trước
            var comicCategories = _context.ComicCategories.Where(cc => cc.ComicId == comic.Id);
            _context.ComicCategories.RemoveRange(comicCategories);

            // Xóa chapter liên quan
            var chapters = _context.Chapters.Where(c => c.ComicId == comic.Id);
            _context.Chapters.RemoveRange(chapters);

            _context.Comics.Remove(comic);
            await _context.SaveChangesAsync();
        }
    }
}
