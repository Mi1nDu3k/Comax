using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Comic;
using Comax.Common.DTOs.Pagination;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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

        // 1. GET BY ID (WITH REDIS CACHE)
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
            catch { /* Lỗi cache không chặn luồng chính */ }

            // B. Gọi DB
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

        // 2. GET BY SLUG (WITH MEMORY CACHE)
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

        // 3. CREATE
        public override async Task<ComicDTO> CreateAsync(ComicCreateDTO dto)
        {
            var entity = _mapper.Map<ComicEntity>(dto);

            // Set mặc định
            entity.Status = "1";
            entity.ViewCount = 0;
            entity.Rating = 0;
            entity.CreatedAt = DateTime.UtcNow;

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
                catch { /* Bỏ qua lỗi parse JSON */ }
            }

            await _comicRepo.AddAsync(entity);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<ComicDTO>(entity);
        }

        // 4. UPDATE (ĐÃ TỐI ƯU - KHÔNG GỌI LẠI DB)
        public override async Task<ComicDTO> UpdateAsync(int id, ComicUpdateDTO dto)
        {
            // 1. Lấy dữ liệu và BẮT BUỘC phải Include ComicCategories
            // Nếu không Include, lệnh Clear() sẽ làm mất quan hệ và có thể gây xóa Cascade
            var entity = await _comicRepo.GetByIdAsync(id);
            if (entity == null) throw new Exception("Không tìm thấy truyện trong hệ thống.");

            // Lưu slug để xóa cache
            string oldSlug = entity.Slug;

            // 2. Map các thông tin cơ bản (Title, Description, AuthorId...)
            // Mapper đã cấu hình Ignore Id nên Id 21 sẽ không bị ghi đè thành 0
            _mapper.Map(dto, entity);

            // 3. XỬ LÝ ẢNH - Tách riêng khỏi Category
            if (dto.CoverImageFile != null)
            {
                // Xóa ảnh cũ trên MinIO (chỉ xóa nếu không phải placeholder)
                if (!string.IsNullOrEmpty(entity.CoverImage) && !entity.CoverImage.Contains("placehold"))
                {
                    await _storageService.DeleteFileAsync(entity.CoverImage);
                }
                // Upload ảnh mới và lưu đường dẫn TƯƠNG ĐỐI
                entity.CoverImage = await _storageService.UploadFileAsync(dto.CoverImageFile, "comics");
            }

            if (dto.CategoryIds != null && dto.CategoryIds.Any())
            {
                
                var allCategories = await _unitOfWork.Categories.GetAllAsync();


                var validCategoryIds = allCategories.Select(c => c.Id).ToList();

                entity.ComicCategories.Clear();

                foreach (var catId in dto.CategoryIds)
                {
               
                    if (validCategoryIds.Contains(catId))
                    {
                        entity.ComicCategories.Add(new ComicCategory
                        {
                            ComicId = id,
                            CategoryId = catId
                        });
                    }
                    else
                    {
                        
                        Console.WriteLine($"[Warning] CategoryId {catId} không tồn tại!");
                    }
                }
            }

            // 5. Cập nhật RowVersion để tránh tranh chấp dữ liệu (Concurrency)
            entity.RowVersion = Guid.NewGuid();
            entity.UpdatedAt = DateTime.UtcNow;

            // 6. Lưu xuống DB
            await _unitOfWork.CommitAsync();

            // 7. Xử lý Cache (Xóa sạch để cập nhật dữ liệu mới nhất)
            await _distCache.RemoveAsync($"comic_id_{id}");
            _memoryCache.Remove($"comic_slug_{oldSlug}");
            _memoryCache.Remove($"comic_slug_{entity.Slug}");

            return _mapper.Map<ComicDTO>(entity);
        }

        // 5. SEARCH
        public async Task<IEnumerable<ComicDTO>> SearchByTitleAsync(string title)
        {
            var filteredEntities = await _comicRepo.SearchByTitleAsync(title);
            return _mapper.Map<IEnumerable<ComicDTO>>(filteredEntities);
        }

        // 6. INCREASE VIEW
        public async Task IncreaseViewCountAsync(int id)
        {
            var comic = await _comicRepo.GetByIdAsync(id);

            if (comic != null)
            {
                comic.ViewCount++;
                // UpdateAsync ở đây an toàn vì comic này không bị map lại từ DTO
                await _comicRepo.UpdateAsync(comic);
                await _unitOfWork.CommitAsync();
            }
        }

        public override async Task<PagedList<ComicDTO>> GetAllPagedAsync(PaginationParams param)
        {
            var pagedEntities = await _comicRepo.GetLatestUpdatedComicsAsync(param);

            var dtos = _mapper.Map<IEnumerable<ComicDTO>>(pagedEntities.Items);

            return new PagedList<ComicDTO>(
                dtos,
                pagedEntities.TotalCount,
                pagedEntities.CurrentPage,
                pagedEntities.PageSize);
        }
    }
}