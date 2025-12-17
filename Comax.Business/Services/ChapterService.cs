using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Chapter;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class ChapterService : BaseService<Chapter, ChapterDTO, ChapterCreateDTO, ChapterUpdateDTO>, IChapterService
    {
        private readonly IChapterRepository _chapterRepo;
        private readonly IComicRepository _comicRepo;
        private readonly IMemoryCache _cache;
        private readonly INotificationService _noticationService;
        
        // Chỉ dùng duy nhất StorageService mới
        private readonly IStorageService _storageService;

        public ChapterService(
            IChapterRepository repo,
            IComicRepository comicRepo,
            IMapper mapper,
            IMemoryCache cache,
            IUnitOfWork unitOfWork,
            INotificationService notiService,
            IStorageService storageService) // Bỏ IMinioService ở đây
            : base(repo, unitOfWork, mapper)
        {
            _chapterRepo = repo;
            _comicRepo = comicRepo;
            _cache = cache;
            _noticationService = notiService;
            _storageService = storageService;
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

        public async Task<ChapterDTO?> GetChapterBySlugsAsync(string comicSlug, string chapterSlug)
        {
            string key = $"chapter_read_{comicSlug}_{chapterSlug}";
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(20);
                var comic = await _comicRepo.GetBySlugAsync(comicSlug);
                if (comic == null) return null;
                var chapter = await _chapterRepo.GetByComicIdAndSlugAsync(comic.Id, chapterSlug);
                if (chapter == null) return null;
                return _mapper.Map<ChapterDTO>(chapter);
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

        public override async Task<ChapterDTO> UpdateAsync(int id, ChapterUpdateDTO dto)
        {
            var result = await base.UpdateAsync(id, dto);
            _cache.Remove($"chapter_id_{id}");
            return result;
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
            // A. Chuẩn bị dữ liệu
            string finalTitle = !string.IsNullOrEmpty(dto.Title)
                                ? dto.Title
                                : $"Chapter {dto.ChapterNumber}";

            string slug = SlugHelper.GenerateSlug(finalTitle);

            // B. Kiểm tra trùng lặp
            var existingChapter = await _chapterRepo.GetByComicIdAndSlugAsync(dto.ComicId, slug);
            if (existingChapter != null)
            {
                throw new Exception($"Chương '{finalTitle}' (Slug: {slug}) đã tồn tại!");
            }

            // C. Upload ảnh lên MinIO (SỬA LẠI ĐOẠN NÀY)
            List<string> uploadedUrls = new List<string>();
            if (dto.Images != null && dto.Images.Count > 0)
            {
                foreach (var img in dto.Images)
                {
                    // Upload từng ảnh một vào folder "comics-bucket"
                    var url = await _storageService.UploadFileAsync(img, "comics-bucket");
                    uploadedUrls.Add(url);
                }
            }

            // D. Tạo Entity Chapter
            var newChapter = new Chapter
            {
                ComicId = dto.ComicId,
                ChapterNumber = dto.ChapterNumber,
                Order = (int)dto.ChapterNumber,
                Title = finalTitle,
                Slug = slug,
                PublishDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                Pages = new List<Page>()
            };

            // E. Tạo các Entity Page từ URL ảnh
            int pageIndex = 0;
            foreach (var url in uploadedUrls)
            {
                var newPage = new Page
                {
                    ImageUrl = url,
                    Index = pageIndex,
                    FileName = Path.GetFileName(url) // Lấy tên file từ URL để lưu
                };

                newChapter.Pages.Add(newPage);
                pageIndex++;
            }

            // F. Lưu vào Database
            await _chapterRepo.AddAsync(newChapter);
            await _unitOfWork.CommitAsync();

            // G. Gửi thông báo
            await SendNotificationsAsync(dto.ComicId, newChapter.Title, newChapter.Slug);

            return _mapper.Map<ChapterDTO>(newChapter);
        }

        // --- Private Helper Method ---
        private async Task SendNotificationsAsync(int comicId, string chapterTitle, string chapterSlug)
        {
            var userIds = await _unitOfWork.Favorites.GetUserIdsByComicIdAsync(comicId);
            var comic = await _comicRepo.GetByIdAsync(comicId);

            if (userIds.Any() && comic != null)
            {
                var notifications = new List<Notification>();
                foreach (var uid in userIds)
                {
                    notifications.Add(new Notification
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
            }
        }
    }
}