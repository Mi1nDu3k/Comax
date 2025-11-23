using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Interfaces
{
    public interface IBaseService<TDto, TCreateDto, TUpdateDto>
        where TDto : Comax.Common.DTOs.BaseDto
    {
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<TDto?> GetByIdAsync(int id);
        Task<TDto> CreateAsync(TCreateDto dto);
        Task<TDto> UpdateAsync(int id, TUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}