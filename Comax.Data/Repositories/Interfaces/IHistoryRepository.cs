using Comax.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Data.Repositories.Interfaces
{
    public interface IHistoryRepository : IBaseRepository<History>
    {
        Task<IEnumerable<History>> GetByUserAsync(int userId);
        Task<History?> GetByUserAndComicAsync(int userId, int comicId);
        void ForceUpdate(History history);
    }
}