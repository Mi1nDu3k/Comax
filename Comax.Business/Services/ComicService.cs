using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Comic;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories; 
using Comax.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Distributed; 
using Microsoft.Extensions.Caching.Memory;     
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

// Alias để tránh nhầm lẫn tên class
using ComicEntity = Comax.Data.Entities.Comic;

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

        // 1. GET BY ID (CÓ CACHE REDIS)
        public override async Task<ComicDTO?> GetByIdAsync(int id)
        {
            string cacheKey = $"comic_id_{id}";

            // A. Kiểm tra Redis Cache trước
            try
            {
                var cachedData = await _distCache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<ComicDTO>(cachedData);
                }
            }
            catch { /* Lỗi cache không được chặn luồng chính */ }

            //Gọi DB
            var entity = await _comicRepo.GetByIdAsync(id);
            if (entity == null) return null;

            var dto = _mapper.Map<ComicDTO>(entity);

            // C. Lưu vào Redis (30 phút)
            try
            {
                var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                await _distCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), options);
            }
            catch { }

            return dto;
        }

        // 2. GET BY SLUG (CÓ CACHE MEMORY - Vì slug ít đổi và cần nhanh)
        public async Task<ComicDTO?> GetBySlugAsync(string slug)
        {
            string key = $"comic_slug_{slug}";

            return await _memoryCache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var entity = await _comicRepo.GetBySlugAsync(slug);
                if (entity == null) return null;

                return _mapper.Map<ComicDTO>(entity);
            });
        }

        // 3. CREATE (UPLOAD ẢNH + AUTO SLUG + CATEGORY + XÓA CACHE LIST)
        public override async Task<ComicDTO> CreateAsync(ComicCreateDTO dto)
        {
            var entity = _mapper.Map<ComicEntity>(dto);

            // Set mặc định
            entity.Status = "1";
            entity.ViewCount = 0;
            entity.Rating = 0;

            // Xử lý ảnh
            if (dto.CoverImageFile != null)
            {
                string imageUrl = await _storageService.UploadFileAsync(dto.CoverImageFile, "comics");
                entity.CoverImage = imageUrl;
            }
            else
            {
                entity.CoverImage = "https://placehold.co/200x300.png";
            }

            // Xử lý Slug
            string slug = SlugHelper.GenerateSlug(dto.Title);
            string originalSlug = slug;
            int count = 0;
            while ((await _comicRepo.GetBySlugAsync(slug)) != null)
            {
                count++;
                slug = $"{originalSlug}-{count}";
            }
            entity.Slug = slug;

            // Xử lý Category JSON
            if (!string.IsNullOrEmpty(dto.CategoryID))
            {
                try
                {
                    var categoryIds = JsonSerializer.Deserialize<List<int>>(dto.CategoryID);
                    if (categoryIds == null || !categoryIds.Any())
                    {
 
                        var strIds = JsonSerializer.Deserialize<List<string>>(dto.CategoryID);
                        categoryIds = strIds?.Select(int.Parse).ToList();
                    }

                    if (categoryIds != null && categoryIds.Any())
                    {
                        entity.ComicCategories = new List<ComicCategory>();
                        foreach (var catId in categoryIds)
                        {
                            entity.ComicCategories.Add(new ComicCategory { CategoryId = catId });
                        }
                    }
                }
                catch { }
            }

            await _comicRepo.AddAsync(entity);
            await _unitOfWork.CommitAsync(); 


            return _mapper.Map<ComicDTO>(entity);
        }

        // 4. UPDATE (UPLOAD + XÓA CACHE CŨ)
        public override async Task<ComicDTO> UpdateAsync(int id, ComicUpdateDTO dto)
        {
            var entity = await _comicRepo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Comic not found");

            string oldSlug = entity.Slug;
            _mapper.Map(dto, entity);

            if (dto.CoverImageFile != null)
            {
                if (!string.IsNullOrEmpty(entity.CoverImage) && !entity.CoverImage.Contains("placehold"))
                {
                    await _storageService.DeleteFileAsync(entity.CoverImage);
                }
                string imageUrl = await _storageService.UploadFileAsync(dto.CoverImageFile, "comics");
                entity.CoverImage = imageUrl;
            }

            await _comicRepo.UpdateAsync(entity);
            await _unitOfWork.CommitAsync();
            await _distCache.RemoveAsync($"comic_id_{id}");
            _memoryCache.Remove($"comic_slug_{oldSlug}");
            if (entity.Slug != oldSlug)
            {
                _memoryCache.Remove($"comic_slug_{entity.Slug}");
            }
            var updatedEntity = await _comicRepo.GetByIdAsync(id);

            return _mapper.Map<ComicDTO>(updatedEntity);
        }

        // 5. SEARCH
        public async Task<IEnumerable<ComicDTO>> SearchByTitleAsync(string title)
        {
            var filteredEntities = await _comicRepo.SearchByTitleAsync(title);
            return _mapper.Map<IEnumerable<ComicDTO>>(filteredEntities);
        }

        // 6. INCREASE VIEW (KHÔNG UPDATE CACHE ĐỂ TỐI ƯU HIỆU NĂNG)
        public async Task IncreaseViewCountAsync(int id)
        {
            ComicEntity? comic = await _comicRepo.GetByIdAsync(id);

            if (comic != null)
            {
                comic.ViewCount++;
                await _comicRepo.UpdateAsync(comic);
                await _unitOfWork.CommitAsync();

            }
        }
    }
}