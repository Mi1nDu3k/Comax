using Comax.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class AuthorRepository : IAuthorRepository
    {
        private readonly ComaxDbContext _context;

        public AuthorRepository(ComaxDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Author>> GetAllAsync()
            => await _context.Authors.ToListAsync();

        public async Task<Author?> GetByIdAsync(int id)
            => await _context.Authors.FirstOrDefaultAsync(a => a.Id == id);

        public async Task AddAsync(Author author)
        {
            await _context.Authors.AddAsync(author);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Author author)
        {
            _context.Authors.Update(author);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Author author)
        {
            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
        }
    }
}
