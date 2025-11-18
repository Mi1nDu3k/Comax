using Comax.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ComaxDbContext _context;

        public CategoryRepository(ComaxDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
            => await _context.Categories.ToListAsync();

        public async Task<Category?> GetByIdAsync(int id)
            => await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
