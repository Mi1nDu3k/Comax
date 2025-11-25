using AutoMapper;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Author;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;

namespace Comax.Business.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _repo;
        private readonly IMapper _mapper;

        public AuthorService(IAuthorRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AuthorDTO>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<AuthorDTO>>(entities);
        }

        public async Task<AuthorDTO?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<AuthorDTO>(entity);
        }

        public async Task<AuthorDTO> CreateAsync(AuthorCreateDTO createDto)
        {
            var dto = createDto as AuthorCreateDTO ?? throw new ArgumentException("Invalid DTO type");
            var entity = _mapper.Map<Author>(dto);
            await _repo.AddAsync(entity);
            return _mapper.Map<AuthorDTO>(entity);
        }

        public async Task<AuthorDTO?> UpdateAsync(int id, AuthorUpdateDTO updateDto)
        {
            var dto = updateDto as AuthorUpdateDTO ?? throw new ArgumentException("Invalid DTO type");
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            await _repo.UpdateAsync(entity);
            return _mapper.Map<AuthorDTO>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return false;

            await _repo.DeleteAsync(category.Id);
            return true;
        }

    }
}
