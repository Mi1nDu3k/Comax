using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class BaseService<TEntity, TDto, TCreateDto, TUpdateDto> : IBaseService<TDto, TCreateDto, TUpdateDto>
        where TDto : Comax.Common.DTOs.BaseDto
        where TEntity : Comax.Data.Entities.BaseEntity
    {
        protected readonly IBaseRepository<TEntity> _repo;
        protected readonly IMapper _mapper;

        public BaseService(IBaseRepository<TEntity> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<TDto> CreateAsync(TCreateDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            await _repo.AddAsync(entity);
            return _mapper.Map<TDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            await _repo.DeleteAsync(entity.Id); // OK vì TEntity có Id
            return true;
        }


        public async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<TDto>>(entities);
        }

        public async Task<TDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<TDto>(entity);
        }

        public async Task<TDto> UpdateAsync(int id, TUpdateDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Entity not found");

            _mapper.Map(dto, entity);
            await _repo.UpdateAsync(entity);
            return _mapper.Map<TDto>(entity);
        }
    }
}
