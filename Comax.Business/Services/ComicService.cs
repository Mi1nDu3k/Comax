using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Comic;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class ComicService : BaseService<Comic, ComicDTO, ComicCreateDTO, ComicUpdateDTO>, IComicService
    {
        private readonly IComicRepository _comicRepo;
        private readonly IStorageService _storageService;
        private readonly IViewCountBuffer _viewBuffer;
        private readonly IMemoryCache _memoryCache;

        public ComicService(
            IComicRepository repo,
            IMapper mapper,
            IStorageService storageService,
            IViewCountBuffer viewBuffer,
            IMemoryCache memoryCache) : base(repo, mapper)
        {
            _comicRepo = repo;
            _storageService = storageService;
            _viewBuffer = viewBuffer;
            _memoryCache = memoryCache;
        }

        // 1. GET BY SLUG (CÓ CACHE)
        public async Task<ComicDTO?> GetBySlugAsync(string slug)
        {
            string key = $"comic_slug_{slug}";

            return await _memoryCache.GetOrCreateAsync(key, async entry =>
            {
                // Cache 10 phút
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var comic = await _comicRepo.GetBySlugAsync(slug);
                if (comic == null) return null;

                return _mapper.Map<ComicDTO>(comic);
            });
        }

        // 2. GET BY ID (OVERRIDE ĐỂ THÊM CACHE)
        public override async Task<ComicDTO?> GetByIdAsync(int id)
        {
            string key = $"comic_id_{id}";

            return await _memoryCache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await base.GetByIdAsync(id);
            });
        }

        // 3. CREATE (UPLOAD ẢNH + AUTO SLUG)
        public override async Task<ComicDTO> CreateAsync(ComicCreateDTO dto)
        {
            // A. Map DTO
            var entity = _mapper.Map<Comic>(dto);

            // B. Upload Ảnh (MinIO)
            if (dto.CoverImageFile != null)
            {
                string imageUrl = await _storageService.UploadFileAsync(dto.CoverImageFile, "comics");
                entity.CoverImage = imageUrl;
            }
            else
            {
                entity.CoverImage = "https://placehold.co/200x300.png"; // Ảnh mặc định
            }

            // C. Tạo Slug chuẩn SEO (Handle trùng lặp)
            string slug = SlugHelper.GenerateSlug(dto.Title);
            string originalSlug = slug;
            int count = 0;

            while ((await _comicRepo.GetBySlugAsync(slug)) != null)
            {
                count++;
                slug = $"{originalSlug}-{count}";
            }
            entity.Slug = slug;

            // D. Lưu vào DB
            await _comicRepo.AddAsync(entity);

            return _mapper.Map<ComicDTO>(entity);
        }

        // 4. UPDATE (UPLOAD ẢNH + XÓA CACHE CŨ)
        public override async Task<ComicDTO> UpdateAsync(int id, ComicUpdateDTO dto)
        {
            var entity = await _comicRepo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Comic not found");

            // Lưu lại slug cũ để xóa cache nếu cần
            string oldSlug = entity.Slug;

            // A. Map thông tin mới
            _mapper.Map(dto, entity);

            // B. Xử lý ảnh mới
            if (dto.CoverImageFile != null)
            {
                // Xóa ảnh cũ trên MinIO nếu không phải ảnh placeholder
                if (!string.IsNullOrEmpty(entity.CoverImage) && !entity.CoverImage.Contains("placehold.co"))
                {
                    await _storageService.DeleteFileAsync(entity.CoverImage);
                }

                // Upload ảnh mới
                string imageUrl = await _storageService.UploadFileAsync(dto.CoverImageFile, "comics");
                entity.CoverImage = imageUrl;
            }

            // C. Cập nhật DB
            await _comicRepo.UpdateAsync(entity);

            // D. INVALIDATE CACHE (Quan trọng: Xóa cache cũ để hiển thị dữ liệu mới)
            _memoryCache.Remove($"comic_id_{id}");
            if (!string.IsNullOrEmpty(oldSlug))
            {
                _memoryCache.Remove($"comic_slug_{oldSlug}");
            }
            // Nếu slug thay đổi thì xóa cả key slug mới (đề phòng)
            if (entity.Slug != oldSlug)
            {
                _memoryCache.Remove($"comic_slug_{entity.Slug}");
            }

            return _mapper.Map<ComicDTO>(entity);
        }

        public async Task<IEnumerable<ComicDTO>> SearchByTitleAsync(string title)
        {
            var filteredEntities = await _comicRepo.SearchByTitleAsync(title);
            return _mapper.Map<IEnumerable<ComicDTO>>(filteredEntities);
        }

        // 5. BUFFER VIEW COUNT
        public async Task IncreaseViewCountAsync(int id)
        {
            _viewBuffer.Increment(id);
            await Task.CompletedTask;
        }
    }
}