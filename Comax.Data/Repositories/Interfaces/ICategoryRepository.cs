using Comax.Data.Entities;

namespace Comax.Data.Repositories.Interfaces
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
        Task<Category?> GetBySlugAsync(string slug);
    }
}
