using Comax.Data.Entities;

namespace Comax.Business.Interfaces
{
    public interface IChapterService
    {
        Task<IEnumerable<Chapter>> GetAllAsync();
        Task<Chapter?> GetByIdAsync(int id);
        Task<IEnumerable<Chapter>> GetByComicIdAsync(int comicId);
        Task AddAsync(Chapter chapter);
        Task UpdateAsync(Chapter chapter);
        Task DeleteAsync(Chapter chapter);
    }
}
