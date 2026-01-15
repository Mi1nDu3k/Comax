using Comax.Common.DTOs.Pagination;
using Comax.Common.DTOs.Comic;
using Comax.Data.Entities;
using ComicEntity = Comax.Data.Entities.Comic;
namespace Comax.Data.Repositories.Interfaces
{
    public interface IComicRepository : IBaseRepository<Comic>
    {
        Task<IEnumerable<Comic>> GetByAuthorIdAsync(int authorId);
        Task<IEnumerable<Comic>> GetByCategoryIdAsync(int categoryId);
        Task<Comic?> GetBySlugAsync(string slug);
        Task<PagedList<Comic>> GetLatestUpdatedComicsAsync(PaginationParams param);
        Task<PagedList<Comic>> GetTrashAsync(PaginationParams param, string searchTerm);
        Task<Comic?> GetDeletedByIdAsync(int id);
        Task<List<Comic>> SearchAsync(string keyword, int limit = 0);
        Task<int> DeleteComicsInTrashOlderThanAsync(DateTime thresholdDate);
        void HardDelete(Comic comic);
        Task<List<Comic>> GetRelatedComicsAsync(int comicId, int limit = 6);

    }
}
