using Comax.Data.Entities;

namespace Comax.Data.Repositories.Interfaces
{
    public interface IAuthorRepository : IBaseRepository<Author>
    {
        Task<Author?> GetByNameAsync(string name);
    }
}
