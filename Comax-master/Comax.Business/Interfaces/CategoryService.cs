using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Common.DTOs;
using Comax.Data.Entities;
using Comax.Data.Repositories;

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

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync()
        {
            var data = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(data);
        }

        public async Task<CategoryDTO?> GetByIdAsync(int id)
        {
            var data = await _repo.GetByIdAsync(id);
            return _mapper.Map<CategoryDTO>(data);
        }

        public async Task<CategoryDTO> CreateAsync(CategoryCreateDTO dto)
        {
            var entity = _mapper.Map<Category>(dto);
            await _repo.AddAsync(entity);
            return _mapper.Map<CategoryDTO>(entity);
        }

        public async Task<bool> UpdateAsync(int id, CategoryUpdateDTO dto)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;

            _mapper.Map(dto, category);
            await _repo.UpdateAsync(category);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;

            await _repo.DeleteAsync(category);
            return true;
        }
    }
}
