using Comax.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Data.Repositories.Interfaces
{
    public interface IFavoriteRepository
    {
        Task AddAsync(Favorite favorite);
        Task RemoveAsync(Favorite favorite);
        Task<Favorite?> GetAsync(int userId, int comicId);
        Task<List<Comic>> GetUserFavoritesAsync(int userId); 
        Task<List<int>> GetUserIdsByComicIdAsync(int comicId);
        Task<List<int>> GetUserIdsByComicIdPagedAsync(int comicId, int lastUserId, int take);
    }
}