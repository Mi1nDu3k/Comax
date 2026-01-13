using Comax.Data.Entities;

namespace Comax.Data.Repositories.Interfaces
{
    public interface IChapterRepository : IBaseRepository<Chapter>
    {
        Task<IEnumerable<Chapter>> GetByComicIdAsync(int comicId);
      
        // Tìm chương dựa trên ID truyện và Slug chương
        Task<Chapter?> GetByComicIdAndSlugAsync(int comicId, string chapterSlug);
        Task<Chapter?> GetByIdAsync(int comicId);
    }
}
