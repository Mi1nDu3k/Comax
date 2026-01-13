using Comax.Common.DTOs.Chapter;
    using Comax.Business.Interfaces;

namespace Comax.Business.Services.Interfaces

{
public interface IChapterService : IBaseService<ChapterDTO, ChapterCreateDTO, ChapterUpdateDTO> {
        Task<ChapterDTO?> GetChapterBySlugsAsync(string comicSlug, string chapterSlug);
        Task<IEnumerable<ChapterDTO>> GetByComicIdAsync(int comicId);
        Task<ChapterDTO> CreateWithImagesAsync(ChapterCreateWithImagesDTO dto);
    }
}
