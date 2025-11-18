using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Common.DTOs;
using Comax.Data.Entities;
using Comax.Data.Repositories;

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
            var authors = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<AuthorDTO>>(authors);
        }

        public async Task<AuthorDTO?> GetByIdAsync(int id)
        {
            var author = await _repo.GetByIdAsync(id);
            return _mapper.Map<AuthorDTO>(author);
        }

        public async Task<AuthorDTO> CreateAsync(AuthorCreateDTO dto)
        {
            var entity = _mapper.Map<Author>(dto);
            await _repo.AddAsync(entity);
            return _mapper.Map<AuthorDTO>(entity);
        }

        public async Task<bool> UpdateAsync(int id, AuthorUpdateDTO dto)
        {
            var author = await _repo.GetByIdAsync(id);
            if (author == null) return false;

            _mapper.Map(dto, author);
            await _repo.UpdateAsync(author);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var author = await _repo.GetByIdAsync(id);
            if (author == null) return false;

            await _repo.DeleteAsync(author);
            return true;
        }
    }
}
