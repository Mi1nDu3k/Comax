using AutoMapper;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Category;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;

namespace Comax.Business.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<CategoryDTO> CreateAsync(CategoryCreateDTO dto)
        {
            var entity = _mapper.Map<Category>(dto);
            await _repo.AddAsync(entity);
            return _mapper.Map<CategoryDTO>(entity);
        }

        public async Task<CategoryDTO> UpdateAsync(int id, CategoryUpdateDTO dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            _mapper.Map(dto, entity);
            await _repo.UpdateAsync(entity);
            return _mapper.Map<CategoryDTO>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            await _repo.DeleteAsync(entity.Id);
            return true;
        }

        public async Task<CategoryDTO?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<CategoryDTO>(entity);
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(entities);
        }
    }
}
