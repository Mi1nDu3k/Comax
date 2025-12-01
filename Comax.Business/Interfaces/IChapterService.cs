using Comax.Common.DTOs.Chapter;
    using Comax.Business.Interfaces;

namespace Comax.Business.Services.Interfaces

{
public interface IChapterService : IBaseService<ChapterDTO, ChapterCreateDTO, ChapterUpdateDTO> {
        Task<ChapterDTO?> GetChapterBySlugsAsync(string comicSlug, string chapterSlug);
    }
}
