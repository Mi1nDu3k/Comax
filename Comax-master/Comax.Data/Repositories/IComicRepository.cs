using Comax.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public interface IComicRepository
    {
        Task<IEnumerable<Comic>> GetAllAsync();
        Task<Comic?> GetByIdAsync(int id);
        Task AddAsync(Comic comic);
    }
    public class ComicRepository : IComicRepository
    {
        private readonly ComaxDbContext _context;
        public ComicRepository(ComaxDbContext context) => _context = context;

        public async Task<IEnumerable<Comic>> GetAllAsync() =>
            await _context.Comics.Include(c => c.Author)
                                 .Include(c => c.Chapters)
                                 .ToListAsync();

        public async Task<Comic?> GetByIdAsync(int id) =>
            await _context.Comics.Include(c => c.Author)
                                 .Include(c => c.Chapters)
                                 .FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(Comic comic)
        {
            await _context.Comics.AddAsync(comic);
            await _context.SaveChangesAsync();
        }
    }
}