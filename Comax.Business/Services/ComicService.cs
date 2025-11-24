using AutoMapper;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Comic;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Comax.Business.Interfaces;

namespace Comax.Business.Services

{
    public class ComicService : IComicService
    {
        private readonly IComicRepository _repo;
        private readonly IMapper _mapper;

        public ComicService(IComicRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        
        public async Task<ComicDTO> CreateAsync(ComicCreateDTO dto)
        {
            var entity = _mapper.Map<Comic>(dto);
            await _repo.AddAsync(entity);
            return _mapper.Map<ComicDTO>(entity);
        }

        public async Task<ComicDTO> UpdateAsync(int id, ComicUpdateDTO dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            _mapper.Map(dto, entity);
            await _repo.UpdateAsync(entity);
            return _mapper.Map<ComicDTO>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            await _repo.DeleteAsync(entity);
            return true;
        }

        public async Task<ComicDTO?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<ComicDTO>(entity);
        }

        public async Task<IEnumerable<ComicDTO>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<ComicDTO>>(entities);
        }
        public async Task<IEnumerable<ComicDTO>> SearchByTitleAsync(string title)
        {
            var comics = await _repo.GetAllAsync();
            var filtered = comics
                .Where(c => c.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            return _mapper.Map<IEnumerable<ComicDTO>>(filtered);
        }
    }

}
