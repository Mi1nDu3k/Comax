    using Comax.Business.Interfaces;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Chapter;

namespace Comax.Business.Services.Interfaces

{
public interface IChapterService : IBaseService<ChapterDTO, ChapterCreateDTO, ChapterUpdateDTO> {
        Task<ChapterDTO?> GetChapterBySlugsAsync(string comicSlug, string chapterSlug);
        Task<IEnumerable<ChapterDTO>> GetByComicIdAsync(int comicId);
        Task<ServiceResponse<ChapterDTO>> CreateWithImagesAsync(ChapterCreateWithImagesDTO dto);
    }
}
