using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.Constants;
using Comax.Common.DTOs.Chapter;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class ChapterService : BaseService<Chapter, ChapterDTO, ChapterCreateDTO, ChapterUpdateDTO>, IChapterService
    {
        private readonly IChapterRepository _chapterRepo;
        private readonly IComicRepository _comicRepo;
        private readonly IMemoryCache _cache;
        private readonly IDistributedCache _distCache;
        private readonly IStorageService _storageService;

        // Dùng ServiceProvider để tạo Scope cho luồng chạy ngầm (Background Task)
        private readonly IServiceProvider _serviceProvider;

        public ChapterService(
            IChapterRepository repo,
            IComicRepository comicRepo,
            IMapper mapper,
            IMemoryCache cache,
            IUnitOfWork unitOfWork,
            IDistributedCache distCache,
            IStorageService storageService,
            IServiceProvider serviceProvider)
            : base(repo, unitOfWork, mapper)
        {
            _chapterRepo = repo;
            _comicRepo = comicRepo;
            _cache = cache;
            _distCache = distCache;
            _storageService = storageService;
            _serviceProvider = serviceProvider;
        }

        // --- CÁC HÀM READ (GIỮ NGUYÊN) ---
        public override async Task<ChapterDTO?> GetByIdAsync(int id)
        {
            string key = $"chapter_id_{id}";
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(20);
                return await base.GetByIdAsync(id);
            });
        }

        public async Task<IEnumerable<ChapterDTO>> GetByComicIdAsync(int comicId)
        {
            var chapters = await _chapterRepo.GetByComicIdAsync(comicId);
            return _mapper.Map<IEnumerable<ChapterDTO>>(chapters.OrderByDescending(x => x.Order));
        }

        public async Task<ChapterDTO?> GetChapterBySlugsAsync(string comicSlug, string chapterSlug)
        {
            string cacheKey = $"chapter_pages_{comicSlug}_{chapterSlug}";

            // 1. Redis Cache
            var cachedData = await _distCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<ChapterDTO>(cachedData);
            }

            // 2. Database
            var comic = await _comicRepo.GetBySlugAsync(comicSlug);
            if (comic == null) return null;

            var chapter = await _chapterRepo.GetByComicIdAndSlugAsync(comic.Id, chapterSlug);
            if (chapter == null) return null;

            var dto = _mapper.Map<ChapterDTO>(chapter);

            // 3. Set Cache (7 ngày)
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(7))
                .SetSlidingExpiration(TimeSpan.FromDays(2));

            await _distCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), options);

            return dto;
        }

        // --- CÁC HÀM WRITE ---

        // 1. TẠO CHƯƠNG CƠ BẢN
        public override async Task<ChapterDTO> CreateAsync(ChapterCreateDTO dto)
        {
            // A. Logic nghiệp vụ
            string slug = SlugHelper.GenerateSlug(dto.Title);
            var existingChapter = await _chapterRepo.GetByComicIdAndSlugAsync(dto.ComicId, slug);
            if (existingChapter != null)
                throw new Exception(string.Format(SystemMessages.Chapter.SlugExists, slug));

            var entity = _mapper.Map<Chapter>(dto);
            entity.Slug = slug;
            entity.PublishDate = DateTime.UtcNow;

            // B. Lưu DB
            await _chapterRepo.AddAsync(entity);
            await _unitOfWork.CommitAsync();

            // C. Gửi thông báo (Chạy nền)
            TriggerNotificationInBackground(dto.ComicId, entity.Title, entity.Slug);

            return _mapper.Map<ChapterDTO>(entity);
        }

        // 2. TẠO CHƯƠNG KÈM ẢNH (QUAN TRỌNG)
        public async Task<ChapterDTO> CreateWithImagesAsync(ChapterCreateWithImagesDTO dto)
        {
            // A. Kiểm tra tồn tại
            string finalTitle = !string.IsNullOrEmpty(dto.Title) ? dto.Title : $"Chapter {dto.ChapterNumber}";
            string slug = SlugHelper.GenerateSlug(finalTitle);

            var existingChapter = await _chapterRepo.GetByComicIdAndSlugAsync(dto.ComicId, slug);
            if (existingChapter != null)
                throw new Exception(string.Format(SystemMessages.Chapter.TitleExists, finalTitle));

            // B. Upload ảnh song song (Parallel Upload)
            List<string> uploadedUrls = new List<string>();
            if (dto.Images != null && dto.Images.Count > 0)
            {
                var uploadTasks = dto.Images.Select(img =>
                    _storageService.UploadFileAsync(img, "comics-bucket")
                ).ToList();

                string[] results = await Task.WhenAll(uploadTasks);
                uploadedUrls.AddRange(results);
            }

            // C. Tạo Entity
            var newChapter = new Chapter
            {
                ComicId = dto.ComicId,
                ChapterNumber = dto.ChapterNumber,
                Order = (int)dto.ChapterNumber,
                Title = finalTitle,
                Slug = slug,
                PublishDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Pages = new List<Page>()
            };

            // Map URLs vào Pages
            for (int i = 0; i < uploadedUrls.Count; i++)
            {
                newChapter.Pages.Add(new Page
                {
                    ImageUrl = uploadedUrls[i],
                    Index = i,
                    FileName = Path.GetFileName(uploadedUrls[i])
                });
            }

            // D. Lưu DB
            await _chapterRepo.AddAsync(newChapter);
            await _unitOfWork.CommitAsync();

            // E. Gửi thông báo (Chạy nền - Fire & Forget)
            // Admin sẽ nhận response ngay lập tức, không cần chờ gửi 1000 thông báo
            TriggerNotificationInBackground(dto.ComicId, newChapter.Title, newChapter.Slug);

            return _mapper.Map<ChapterDTO>(newChapter);
        }

        // --- HELPER: BẮN THÔNG BÁO NGẦM ---
        private void TriggerNotificationInBackground(int comicId, string chapterTitle, string chapterSlug)
        {
            // Task.Run để tách ra luồng riêng
            _ = Task.Run(async () =>
            {
                try
                {
                    // Tạo Scope mới vì DbContext là Scoped, không thể dùng lại Scope của Request cũ đã kết thúc
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var notiService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        var comicRepo = scope.ServiceProvider.GetRequiredService<IComicRepository>();

                        // Lấy thông tin cần thiết
                        var comic = await comicRepo.GetByIdAsync(comicId);
                        var followerIds = await unitOfWork.Favorites.GetUserIdsByComicIdAsync(comicId);

                        if (followerIds.Any() && comic != null)
                        {
                            // Gọi hàm Batch Processing đã viết ở NotificationService
                            await notiService.SendNotificationToGroupAsync(
                                followerIds,
                               string.Format(SystemMessages.Notification.NewChapter, comic.Title, chapterTitle),
                                $"/comics/{comic.Slug}/chapter/{chapterSlug}"
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi nếu gửi thất bại (vì chạy ngầm nên không throw ra ngoài được)
                    Console.WriteLine($"Background Notification Error: {ex.Message}");
                }
            });
        }

        public override async Task<ChapterDTO> UpdateAsync(int id, ChapterUpdateDTO dto)
        {
            var entity = await _unitOfWork.Chapters.GetByIdAsync(id);
            if (entity == null) throw new Exception(SystemMessages.Chapter.NotFound);
            _mapper.Map(dto, entity);

            await _unitOfWork.CommitAsync();
            return _mapper.Map<ChapterDTO>(entity);
        }

        public override async Task<bool> DeleteAsync(int id, bool hardDelete = false)
        {
            var result = await base.DeleteAsync(id, hardDelete);
            if (result)
            {
                _cache.Remove($"chapter_id_{id}");
            }
            return result;
        }
    }
}