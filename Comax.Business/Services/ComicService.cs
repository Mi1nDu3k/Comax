using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.Constants;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Comic;
using Comax.Common.DTOs.Pagination;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories;
using Comax.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
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

        // --- 1. GET BY ID (CÓ REDIS CACHE) ---
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

            // ---> [QUAN TRỌNG] Map thủ công danh sách tên thể loại <---
            if (entity.ComicCategories != null)
            {
                dto.CategoryNames = entity.ComicCategories
                    .Select(cc => cc.Category?.Name)
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();
            }

            // C. Lưu vào Redis (30 phút)
            try
            {
                var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                await _distCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), options);
            }
            catch { }

            return dto;
        }

        // --- 2. GET BY SLUG (CÓ MEMORY CACHE) -> Dùng cho trang Detail ---
        public async Task<ComicDTO?> GetBySlugAsync(string slug)
        {
            string key = $"comic_slug_{slug}";

            return await _memoryCache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var entity = await _comicRepo.GetBySlugAsync(slug);
                if (entity == null) return null;

                var dto = _mapper.Map<ComicDTO>(entity);

                // ---> [QUAN TRỌNG] Map thủ công danh sách tên thể loại <---
                if (entity.ComicCategories != null)
                {
                    dto.CategoryNames = entity.ComicCategories
                        .Select(cc => cc.Category?.Name)
                        .Where(n => !string.IsNullOrEmpty(n))
                        .ToList();
                }

                return dto;
            });
        }

        // --- 3. CREATE ---
        public override async Task<ComicDTO> CreateAsync(ComicCreateDTO dto)
        {
            var entity = _mapper.Map<ComicEntity>(dto);

            // Set mặc định
            entity.Status = "1"; // Đang tiến hành
            entity.ViewCount = 0;
            entity.Rating = 0;
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            if (dto.CoverImageFile != null)
            {
                entity.CoverImage = await _storageService.UploadFileAsync(dto.CoverImageFile, "comics");
            }
            else
            {
                entity.CoverImage = "https://placehold.co/200x300.png";
            }
            string slug = SlugHelper.GenerateSlug(dto.Title);
            string originalSlug = slug;
            int count = 0;
            while ((await _comicRepo.GetBySlugAsync(slug)) != null)
            {
                count++;
                slug = $"{originalSlug}-{count}";
            }
            entity.Slug = slug;
            if (!string.IsNullOrEmpty(dto.CategoryID))
            {
                try
                {
                    List<int>? categoryIds = null;
                    // Thử parse JSON
                    try
                    {
                        categoryIds = JsonSerializer.Deserialize<List<int>>(dto.CategoryID);
                    }
                    catch
                    {
                        // Nếu JSON lỗi, thử parse List string
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

        // --- 4. UPDATE ---
        public override async Task<ComicDTO> UpdateAsync(int id, ComicUpdateDTO dto)
        {
            var entity = await _comicRepo.GetByIdAsync(id);
            if (entity == null) throw new Exception(SystemMessages.Comic.NotFound);

            string oldSlug = entity.Slug;

            _mapper.Map(dto, entity);

            // Xử lý ảnh mới
            if (dto.CoverImageFile != null)
            {
                // Xóa ảnh cũ nếu không phải ảnh placeholder
                if (!string.IsNullOrEmpty(entity.CoverImage) && !entity.CoverImage.Contains("placehold"))
                {
                    await _storageService.DeleteFileAsync(entity.CoverImage);
                }
                entity.CoverImage = await _storageService.UploadFileAsync(dto.CoverImageFile, "comics");
            }

            // Xử lý cập nhật danh sách thể loại
            if (dto.CategoryIds != null) // Chỉ update nếu client có gửi list này lên
            {
                var allCategories = await _unitOfWork.Categories.GetAllAsync();
                var validCategoryIds = allCategories.Select(c => c.Id).ToList();

                // Xóa cũ thêm mới (Sync)
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
                }
            }

            // Cập nhật Audit
            entity.RowVersion = Guid.NewGuid();
            entity.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CommitAsync();

            // Xóa Cache để User thấy dữ liệu mới
            await _distCache.RemoveAsync($"comic_id_{id}");
            _memoryCache.Remove($"comic_slug_{oldSlug}");
            _memoryCache.Remove($"comic_slug_{entity.Slug}");

            // Trả về DTO kèm CategoryNames mới
            var resultDto = _mapper.Map<ComicDTO>(entity);
            if (entity.ComicCategories != null)
            {
                resultDto.CategoryNames = entity.ComicCategories
                   .Select(cc => cc.Category?.Name)
                   .Where(n => !string.IsNullOrEmpty(n))
                   .ToList();
            }
            return resultDto;
        }

        // --- 5. SEARCH ---
        public async Task<List<ComicDTO>> SearchComics(string keyword, int limit = 0)
        {
 
            var entities = await _comicRepo.SearchAsync(keyword, limit);

            var dtos = _mapper.Map<List<ComicDTO>>(entities);

           
            var entitiesList = entities.ToList();
            for (int i = 0; i < dtos.Count; i++)
            {
                if (entitiesList[i].ComicCategories != null)
                {
                    dtos[i].CategoryNames = entitiesList[i].ComicCategories
                        .Select(cc => cc.Category?.Name)
                        .Where(n => !string.IsNullOrEmpty(n))
                        .ToList();
                }
            }

            return dtos;
        }

        // --- 6. INCREASE VIEW ---
        public async Task IncreaseViewCountAsync(int id)
        {
            // Lưu ý: Hàm này gọi trực tiếp Repo để update nhanh, không qua Mapper
            var comic = await _comicRepo.GetByIdAsync(id);

            if (comic != null)
            {
                comic.ViewCount++;
                // Dùng Update của Repo
                await _comicRepo.UpdateAsync(comic);
                await _unitOfWork.CommitAsync();
            }
        }

        // --- 7. GET ALL PAGED (Trang chủ/Danh sách) ---
        public override async Task<PagedList<ComicDTO>> GetAllPagedAsync(PaginationParams param)
        {
            // Lấy dữ liệu từ Repo (Đã có Include Categories)
            var pagedEntities = await _comicRepo.GetLatestUpdatedComicsAsync(param);

            // Map sang List DTO
            var dtos = _mapper.Map<IEnumerable<ComicDTO>>(pagedEntities.Items).ToList();

            // ---> [QUAN TRỌNG] Loop qua từng item để map CategoryNames cho danh sách <---
            var entitiesList = pagedEntities.Items.ToList();
            for (int i = 0; i < dtos.Count; i++)
            {
                if (entitiesList[i].ComicCategories != null)
                {
                    dtos[i].CategoryNames = entitiesList[i].ComicCategories
                        .Select(cc => cc.Category?.Name)
                        .Where(n => !string.IsNullOrEmpty(n))
                        .ToList();
                }
            }

            return new PagedList<ComicDTO>(
                dtos,
                pagedEntities.TotalCount,
                pagedEntities.CurrentPage,
                pagedEntities.PageSize);
        }

        // --- 8. GET TRASH ---
        public async Task<PagedList<ComicDTO>> GetTrashAsync(PaginationParams param, string searchTerm)
        {
            var pagedData = await _comicRepo.GetTrashAsync(param, searchTerm);
            var dtos = _mapper.Map<IEnumerable<ComicDTO>>(pagedData.Items);

            return new PagedList<ComicDTO>(
                dtos,
                pagedData.TotalCount,
                pagedData.CurrentPage,
                pagedData.PageSize
            );
        }

        // --- 9. RESTORE ---
        public async Task<bool> RestoreAsync(int id)
        {
            var comic = await _comicRepo.GetDeletedByIdAsync(id);
            if (comic == null) return false;

            comic.IsDeleted = false;
            comic.UpdatedAt = DateTime.UtcNow;

            await _comicRepo.UpdateAsync(comic);
            return await _unitOfWork.CommitAsync() > 0;
        }

        // --- 10. PURGE (XÓA VĨNH VIỄN) ---
        public async Task<bool> PurgeAsync(int id)
        {
            var comic = await _comicRepo.GetDeletedByIdAsync(id);
            if (comic == null) return false;

            // Xóa ảnh trên MinIO để tiết kiệm dung lượng
            if (!string.IsNullOrEmpty(comic.CoverImage) && !comic.CoverImage.Contains("placehold"))
            {
                try
                {
                    await _storageService.DeleteFileAsync(comic.CoverImage);
                }
                catch { /* Bỏ qua lỗi xóa file */ }
            }

            _comicRepo.HardDelete(comic);
            return await _unitOfWork.CommitAsync() > 0;
        }
        public async Task<List<ComicDTO>> GetRelatedAsync(int id)
        {
            // Gọi Repo lấy Entity
            var comics = await _comicRepo.GetRelatedComicsAsync(id);

            // Map sang DTO để trả về Frontend
            return _mapper.Map<List<ComicDTO>>(comics);
        }

    }
}