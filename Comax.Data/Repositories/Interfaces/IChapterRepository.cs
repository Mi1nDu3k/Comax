using Comax.Data.Entities;

namespace Comax.Data.Repositories.Interfaces
{
    public interface IChapterRepository : IBaseRepository<Chapter>
    {
        Task<IEnumerable<Chapter>> GetByComicIdAsync(int comicId);
    }
}
