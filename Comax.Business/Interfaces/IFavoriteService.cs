using Comax.Common.DTOs.Comic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Interfaces
{
    public interface IFavoriteService

    {
        Task<bool> ToggleFavoriteAsync(int userId, int comicId); // True = Added, False = Removed
        Task<List<ComicDTO>> GetUserFavoritesAsync(int userId);
        Task<bool> IsFavoritedAsync(int userId, int comicId);
        Task UnfavoriteAsync(int userId, int comicId);
        Task<List<ComicDTO>> GetFavoritesByUserIdAsync(int userId);
    }
}
