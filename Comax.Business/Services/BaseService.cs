using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Comax.Data.Entities;
using System;
using Comax.Common.DTOs.Pagination; // Cần import

namespace Comax.Business.Services
{
    public class BaseService<TEntity, TDto, TCreateDto, TUpdateDto> : IBaseService<TDto, TCreateDto, TUpdateDto>
        where TDto : Comax.Common.DTOs.BaseDto
        where TEntity : Comax.Data.Entities.BaseEntity
    {
        protected readonly IBaseRepository<TEntity> _repo;
        protected readonly IUnitOfWork _unitOfWork; 
        protected readonly IMapper _mapper;

        public BaseService(IBaseRepository<TEntity> repo, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _repo = repo;
            _unitOfWork = unitOfWork; 
            _mapper = mapper;
        }
        // THÊM: Implementation phân trang
        public virtual async Task<PagedList<TDto>> GetAllPagedAsync(PaginationParams @params)
        {
            var (entities, totalCount) = await _repo.GetAllPagedAsync(@params.PageNumber, @params.PageSize);

            var dtos = _mapper.Map<IEnumerable<TDto>>(entities);

            return new PagedList<TDto>(
                dtos,
                totalCount,
                @params.PageNumber,
                @params.PageSize
            );
        }


        public virtual async Task<TDto> CreateAsync(TCreateDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            await _repo.AddAsync(entity); 
            await _unitOfWork.CommitAsync(); 
            return _mapper.Map<TDto>(entity);
        }
        public virtual async Task<bool> DeleteAsync(int id, bool hardDelete = false)
        {
            var result = await _repo.DeleteAsync(id, hardDelete);
            if (result) await _unitOfWork.CommitAsync(); // Base tự động Commit
            return result;
        }

        public virtual async Task<IEnumerable<TDto>> GetAllAsync()
        {
            var entities = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<TDto>>(entities);
        }

        public virtual async Task<TDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return _mapper.Map<TDto>(entity);
        }

        public virtual async Task<TDto> UpdateAsync(int id, TUpdateDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Entity not found");

            _mapper.Map(dto, entity);
            await _repo.UpdateAsync(entity);
            await _unitOfWork.CommitAsync(); 
            return _mapper.Map<TDto>(entity);
        }
    }
    }