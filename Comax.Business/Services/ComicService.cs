using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Comic;
using Comax.Common.DTOs.Pagination; 
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore; 
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PagedListHelper = Comax.Common.Helpers.PagedList<Comax.Data.Entities.Comic>;

// 2. ComicEntity: Đặt tên ngắn gọn cho Entity
using ComicEntity = Comax.Data.Entities.Comic;
using Comax.Common.Helpers;

namespace Comax.Business.Services
{
    public class ComicService : BaseService<ComicEntity, ComicDTO, ComicCreateDTO, ComicUpdateDTO>, IComicService
    {
        private readonly IComicRepository _comicRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distCache;

        public ComicService(
            IComicRepository repo,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IStorageService storageService,
            IMemoryCache memoryCache,
            IDistributedCache distCache)
            : base(repo, unitOfWork, mapper)
        {
            _comicRepo = repo;
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _memoryCache = memoryCache;
            _distCache = distCache;
        }

        // ... (Giữ nguyên các hàm GetById, GetBySlug, Create, Update, Search...)
        public override async Task<ComicDTO?> GetByIdAsync(int id)
        {
            // (Giữ nguyên logic cũ của bạn)
            return await base.GetByIdAsync(id);
        }

        public async Task<ComicDTO?> GetBySlugAsync(string slug)
        {
            // (Giữ nguyên logic cũ của bạn)
            var entity = await _comicRepo.GetBySlugAsync(slug);
            return _mapper.Map<ComicDTO>(entity);
        }

        public override async Task<ComicDTO> CreateAsync(ComicCreateDTO dto)
        {
            // (Copy lại logic Create cũ của bạn vào đây nếu cần, hoặc để base xử lý nếu không custom)
            // Ở code cũ bạn có logic custom (upload ảnh, slug...), hãy giữ nguyên nó.
            return await base.CreateAsync(dto);
        }

        public override async Task<ComicDTO> UpdateAsync(int id, ComicUpdateDTO dto)
        {
            // (Giữ nguyên logic custom Update của bạn)
            return await base.UpdateAsync(id, dto);
        }

        public async Task<IEnumerable<ComicDTO>> SearchByTitleAsync(string title)
        {
            var filteredEntities = await _comicRepo.SearchByTitleAsync(title);
            return _mapper.Map<IEnumerable<ComicDTO>>(filteredEntities);
        }

        public async Task IncreaseViewCountAsync(int id)
        {
            var comic = await _comicRepo.GetByIdAsync(id);
            if (comic != null)
            {
                comic.ViewCount++;
                await _comicRepo.UpdateAsync(comic);
                await _unitOfWork.CommitAsync();
            }
        }

        
        public async Task<PagedList<ComicDTO>> GetAllPagedAsync(PaginationParams paginationParams)
        {
            // 1. Tạo Query
            IQueryable<ComicEntity> query = _unitOfWork.Comics.FindAll(trackChanges: false)
                                            .Include(c => c.ComicCategories);

            // 2. Search
            if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
            {
                var lowerTerm = paginationParams.SearchTerm.ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(lowerTerm));
            }

            // 3. Filter Category
            if (paginationParams.CategoryIds != null && paginationParams.CategoryIds.Count > 0)
            {
                query = query.Where(c => c.ComicCategories.Any(cc => paginationParams.CategoryIds.Contains(cc.CategoryId)));
            }

            // 4. Sort
            query = query.OrderByDescending(c => c.UpdatedAt);

            // 5. Phân trang Database (Sử dụng Alias PagedListHelper)
            // pagedEntities lúc này là Helper.PagedList chứa các Entity
            var pagedEntities = await PagedListHelper.ToPagedListAsync(
                query,
                paginationParams.PageNumber,
                paginationParams.PageSize
            );

            // 6. Map Entity sang DTO
            // SỬA LỖI: Map thẳng sang List<ComicDTO> để khớp với Constructor
            var dtos = _mapper.Map<List<ComicDTO>>(pagedEntities);

            // 7. Trả về PagedList DTO (Comax.Common.DTOs.Pagination)
            return new PagedList<ComicDTO>(
                dtos,
                pagedEntities.TotalCount,
                pagedEntities.CurrentPage,
                pagedEntities.PageSize
            );
        }
    }
}