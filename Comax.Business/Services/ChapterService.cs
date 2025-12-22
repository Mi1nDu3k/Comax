using AutoMapper;
using Comax.API.Hubs;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Chapter;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly INotificationService _noticationService;
        private readonly IDistributedCache _distCache;
        // Chỉ dùng duy nhất StorageService mới
        private readonly IStorageService _storageService;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ChapterService(
            IChapterRepository repo,
            IComicRepository comicRepo,
            IMapper mapper,
            IHubContext<NotificationHub> hubContext,
            IMemoryCache cache,
            IUnitOfWork unitOfWork,
            IDistributedCache distCache,
            INotificationService notiService,
            IStorageService storageService) // Bỏ IMinioService ở đây
            : base(repo, unitOfWork, mapper)
        {
            _chapterRepo = repo;
            _comicRepo = comicRepo;
            _cache = cache;
            _distCache = distCache;
            _noticationService = notiService;
            _storageService = storageService;
            _hubContext = hubContext;
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

        // --- CÁC HÀM WRITE ---

        public override async Task<ChapterDTO> CreateAsync(ChapterCreateDTO dto)
        {
            string slug = SlugHelper.GenerateSlug(dto.Title);

            var existingChapter = await _chapterRepo.GetByComicIdAndSlugAsync(dto.ComicId, slug);
            if (existingChapter != null)
            {
                throw new Exception($"Slug '{slug}' đã tồn tại trong truyện này.");
            }

            var entity = _mapper.Map<Chapter>(dto);
            entity.Slug = slug;
            entity.PublishDate = DateTime.UtcNow;

            await _chapterRepo.AddAsync(entity);
            await _unitOfWork.CommitAsync();

            // Gửi thông báo
            await SendNotificationsAsync(dto.ComicId, entity.Title, entity.Slug);

            return _mapper.Map<ChapterDTO>(entity);
        }
        private async Task PrepareNotificationsAsync(Chapter chapter, int comicId)
        {
            // Lấy danh sách ID người dùng đã theo dõi truyện này
            var userIds = await _unitOfWork.Favorites.GetUserIdsByComicIdAsync(comicId);
            var comic = await _comicRepo.GetByIdAsync(comicId);

            if (userIds.Any() && comic != null)
            {
                foreach (var uid in userIds)
                {
                    var noti = new Data.Entities.Notification
                    {
                        UserId = uid,
                        Message = $"Truyện '{comic.Title}' vừa có chương mới: {chapter.Title}",
                        Url = $"/comic/{comic.Slug}/chapter/{chapter.Slug}",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    // QUAN TRỌNG: Chỉ thêm vào Repo, KHÔNG gọi CommitAsync ở đây
                    await _unitOfWork.Notifications.AddAsync(noti);
                }
            }
        }
        public override async Task<ChapterDTO> UpdateAsync(int id, ChapterUpdateDTO dto)
        {
            var entity = await _unitOfWork.Chapters.GetByIdAsync(id);
            if (entity == null) throw new Exception("Không tìm thấy chương.");
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

        // --- HÀM TẠO CHƯƠNG KÈM ẢNH (QUAN TRỌNG ĐÃ SỬA) ---
        public async Task<ChapterDTO> CreateWithImagesAsync(ChapterCreateWithImagesDTO dto)
        {
            // A. Chuẩn bị dữ liệu và kiểm tra (Giữ nguyên logic cũ)
            string finalTitle = !string.IsNullOrEmpty(dto.Title) ? dto.Title : $"Chapter {dto.ChapterNumber}";
            string slug = SlugHelper.GenerateSlug(finalTitle);

            var existingChapter = await _chapterRepo.GetByComicIdAndSlugAsync(dto.ComicId, slug);
            if (existingChapter != null)
            {
                throw new Exception($"Chương '{finalTitle}' (Slug: {slug}) đã tồn tại!");
            }

            // B. TẢI ẢNH SONG SONG (Đây là phần thay đổi quan trọng)
            List<string> uploadedUrls = new List<string>();
            if (dto.Images != null && dto.Images.Count > 0)
            {
                // 1. Khởi tạo danh sách các Task (chưa chạy await ngay)
                var uploadTasks = dto.Images.Select(img =>
                    _storageService.UploadFileAsync(img, "comics-bucket")
                ).ToList();

                // 2. Kích hoạt tất cả Task cùng lúc và đợi toàn bộ hoàn thành
                string[] results = await Task.WhenAll(uploadTasks);

                // 3. Gom tất cả URL trả về
                uploadedUrls.AddRange(results);
            }

            // C. Tạo Entity Chapter và Pages
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

            // Tạo các trang dựa trên thứ tự URL đã upload
            for (int i = 0; i < uploadedUrls.Count; i++)
            {
                newChapter.Pages.Add(new Page
                {
                    ImageUrl = uploadedUrls[i],
                    Index = i,
                    FileName = Path.GetFileName(uploadedUrls[i])
                });
            }

            // D. Lưu vào Database và gửi thông báo trong 1 Transaction duy nhất
            await _chapterRepo.AddAsync(newChapter);

            // Tối ưu: Thêm thông báo vào cùng UnitOfWork trước khi Commit
            await PrepareNotificationsAsync(newChapter, dto.ComicId);

            // Lưu tất cả (Chapter, Pages, Notifications) chỉ với 1 lần gọi DB
            await _unitOfWork.CommitAsync();

            return _mapper.Map<ChapterDTO>(newChapter);
        }

        // --- Private Helper Method ---
        private async Task SendNotificationsAsync(int comicId, string chapterTitle, string chapterSlug)
        {
            var userIds = await _unitOfWork.Favorites.GetUserIdsByComicIdAsync(comicId);
            var comic = await _comicRepo.GetByIdAsync(comicId);

            if (userIds.Any() && comic != null)
            {
                var notifications = new List<Data.Entities.Notification>();
                foreach (var uid in userIds)
                {
                    notifications.Add(new Data.Entities.Notification
                    {
                        UserId = uid,
                        Message = $"Truyện '{comic.Title}' vừa có chương mới: {chapterTitle}",
                        Url = $"/comic/{comic.Slug}/chapter/{chapterSlug}",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                foreach (var noti in notifications)
                {
                    await _unitOfWork.Notifications.AddAsync(noti);
                }


                await _unitOfWork.CommitAsync();
                var tasks = userIds.Select(uid =>
                    _hubContext.Clients.User(uid.ToString()).SendAsync("ReceiveNotification", new
                    {
                        message = $"Truyện '{comic.Title}' vừa có chương mới: {chapterTitle}",
                        url = $"/comic/{comic.Slug}/chapter/{chapterSlug}",
                        createdAt = DateTime.UtcNow
                    })
                );

                await Task.WhenAll(tasks);
            }
        }
        public async Task<ChapterDTO?> GetChapterBySlugsAsync(string comicSlug, string chapterSlug)
        {
            // Tạo Key định danh duy nhất cho nội dung chương này
            string cacheKey = $"chapter_pages_{comicSlug}_{chapterSlug}";

            // 1. Kiểm tra trong Redis
            var cachedData = await _distCache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<ChapterDTO>(cachedData);
            }

            // 2. Nếu Cache Miss, lấy từ DB (nhớ Include Pages để có danh sách ảnh)
            var comic = await _comicRepo.GetBySlugAsync(comicSlug);
            if (comic == null) return null;

            var chapter = await _chapterRepo.GetByComicIdAndSlugAsync(comic.Id, chapterSlug);
            if (chapter == null) return null;

            // Mapping: AutoMapper sẽ tự nối baseUrl vào các ImageUrl tương đối
            var dto = _mapper.Map<ChapterDTO>(chapter);

            // 3. Lưu vào Redis với thời hạn 7 ngày
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(7)) // Xóa hẳn sau 7 ngày
                .SetSlidingExpiration(TimeSpan.FromDays(2)); // Nếu 2 ngày không ai đọc thì xóa sớm để tiết kiệm RAM

            await _distCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto), options);

            return dto;
        }
    }
}