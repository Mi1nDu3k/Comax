using Comax.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly ComaxDbContext _context;
        public ChapterRepository(ComaxDbContext context) => _context = context;

        public async Task<IEnumerable<Chapter>> GetAllAsync() =>
            await _context.Chapters.Include(c => c.Comic).ToListAsync();

        public async Task<Chapter?> GetByIdAsync(int id) =>
            await _context.Chapters.Include(c => c.Comic)
                                   .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<IEnumerable<Chapter>> GetByComicIdAsync(int comicId) =>
            await _context.Chapters.Where(c => c.ComicId == comicId)
                                   .OrderBy(c => c.Order)
                                   .ToListAsync();

        public async Task AddAsync(Chapter chapter)
        {
            await _context.Chapters.AddAsync(chapter);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Chapter chapter)
        {
            _context.Chapters.Update(chapter);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Chapter chapter)
        {
            _context.Chapters.Remove(chapter);
            await _context.SaveChangesAsync();
        }
    }
}