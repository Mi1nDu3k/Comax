using Comax.Common.DTOs.Pagination;
using Comax.Data.Entities;
using ComicEntity = Comax.Data.Entities.Comic;
namespace Comax.Data.Repositories.Interfaces
{
    public interface IComicRepository : IBaseRepository<Comic>
    {
        Task<IEnumerable<Comic>> SearchByTitleAsync(string title);
        Task<IEnumerable<Comic>> GetByAuthorIdAsync(int authorId);
        Task<IEnumerable<Comic>> GetByCategoryIdAsync(int categoryId);
        Task<Comic?> GetBySlugAsync(string slug);
        Task<PagedList<Comic>> GetLatestUpdatedComicsAsync(PaginationParams param);
    }
}
