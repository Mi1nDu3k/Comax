using Comax.Common.DTOs;

namespace Comax.Business.Interfaces
{
    public interface IAuthorService
    {
        Task<IEnumerable<AuthorDTO>> GetAllAsync();
        Task<AuthorDTO?> GetByIdAsync(int id);
        Task<AuthorDTO> CreateAsync(AuthorCreateDTO dto);
        Task<bool> UpdateAsync(int id, AuthorUpdateDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
