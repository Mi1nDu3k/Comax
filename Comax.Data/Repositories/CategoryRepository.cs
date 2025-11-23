using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Comax.Data.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ComaxDbContext context) : base(context) { }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
        }
    }
}
