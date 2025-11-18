using Comax.Data.Entities;

namespace Comax.Business.Interfaces
{
    public interface IComicService
    {
        Task<IEnumerable<Comic>> GetAllAsync();
        Task<Comic?> GetByIdAsync(int id);
        Task<IEnumerable<Comic>> SearchByTitleAsync(string title);
        Task<IEnumerable<Comic>> GetByAuthorIdAsync(int authorId);
        Task<IEnumerable<Comic>> GetByCategoryIdAsync(int categoryId);
        Task AddAsync(Comic comic, List<int>? categoryIds = null);
        Task UpdateAsync(Comic comic, List<int>? categoryIds = null);
        Task DeleteAsync(Comic comic);
    }
}
