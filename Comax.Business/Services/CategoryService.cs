using AutoMapper;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Category;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Comax.Business.Interfaces;
using Comax.Common.DTOs.Pagination;
using Comax.Common.Helpers;

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

        public async Task<CategoryDTO?> GetBySlugAsync(string slug)
        {
            var category = await _repo.GetBySlugAsync(slug);
            if (category == null) return null;
            return _mapper.Map<CategoryDTO>(category);
        }

        // Override Create: Tự động tạo slug
        public override async Task<CategoryDTO> CreateAsync(CategoryCreateDTO dto)
        {
            string slug = SlugHelper.GenerateSlug(dto.Name);

            //  Kiểm tra trùng lặp
            string originalSlug = slug;
            int count = 0;
            while ((await _repo.GetBySlugAsync(slug)) != null)
            {
                count++;
                slug = $"{originalSlug}-{count}";
            }

            // Map và Lưu
            var entity = _mapper.Map<Category>(dto);
            entity.Slug = slug; 

            await _repo.AddAsync(entity);
            return _mapper.Map<CategoryDTO>(entity);
        }
    }
}