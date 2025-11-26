using Comax.Common.DTOs.Comic;
using Comax.Business.Interfaces;


namespace Comax.Business.Services.Interfaces
{
    public interface IComicService : IBaseService<ComicDTO, ComicCreateDTO, ComicUpdateDTO> {
        Task<IEnumerable<ComicDTO>> SearchByTitleAsync(string title);
        Task IncreaseViewCountAsync(int id);
    }
}
