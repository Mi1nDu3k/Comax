using Comax.Business.Interfaces;
using Comax.Common.DTOs.Comic;
using Comax.Common.DTOs.Pagination;


namespace Comax.Business.Services.Interfaces
{
    public interface IComicService : IBaseService<ComicDTO, ComicCreateDTO, ComicUpdateDTO> {
        Task<ComicDTO?> GetBySlugAsync(string slug);
        Task IncreaseViewCountAsync(int id);
        Task<PagedList<ComicDTO>> GetTrashAsync(PaginationParams param, string searchTerm);
        Task<bool> RestoreAsync(int id);
        Task<List<ComicDTO>> GetRelatedAsync(int id);
        Task<bool> PurgeAsync(int id);

        Task<List<ComicDTO>> SearchComics(string keyword, int limit = 0);
    }
}
