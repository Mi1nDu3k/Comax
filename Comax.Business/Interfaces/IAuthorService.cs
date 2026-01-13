using Comax.Business.Interfaces;
using Comax.Common.DTOs.Author;
using Comax.Common.DTOs.User;

namespace Comax.Business.Services.Interfaces
{
    public interface IAuthorService : IBaseService<AuthorDTO, AuthorCreateDTO, AuthorUpdateDTO> { }
}
