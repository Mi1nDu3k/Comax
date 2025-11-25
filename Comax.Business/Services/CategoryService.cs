using AutoMapper;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Category;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Comax.Business.Interfaces;
using Comax.Common.DTOs.Pagination;

namespace Comax.Business.Services
{
    // CẬP NHẬT: Kế thừa BaseService và triển khai ICategoryService
    public class CategoryService : BaseService<Category, CategoryDTO, CategoryCreateDTO, CategoryUpdateDTO>, ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // TẤT CẢ các phương thức CRUD được KẾ THỪA. Xóa implementation bị trùng lặp.
    }
}