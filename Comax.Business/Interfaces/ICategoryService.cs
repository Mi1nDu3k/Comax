using Comax.Common.DTOs.Category;
using Comax.Business.Interfaces;


namespace Comax.Business.Services.Interfaces
{
    public interface ICategoryService : IBaseService<CategoryDTO, CategoryCreateDTO, CategoryUpdateDTO> { }
}
