using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Chapter;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class ChapterService : BaseService<Chapter, ChapterDTO, ChapterCreateDTO, ChapterUpdateDTO>, IChapterService
    {
        private readonly IChapterRepository _chapterRepo;
        private readonly IComicRepository _comicRepo;
        private readonly IMemoryCache _cache;
        private readonly INotificationService _noticationService;

        public ChapterService(
            IChapterRepository repo,
            IComicRepository comicRepo,
            IMapper mapper,
            IMemoryCache cache,
            IUnitOfWork unitOfWork,
            INotificationService notiService)
            : base(repo, unitOfWork, mapper)
        {
            _chapterRepo = repo;
            _comicRepo = comicRepo;
            _cache = cache;
            _noticationService=notiService;
        }

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

        // --- WRITE (Dùng UnitOfWork) ---

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
            var userIds = await _unitOfWork.Favorites.GetUserIdsByComicIdAsync(dto.ComicId);
            var comic = await _comicRepo.GetByIdAsync(dto.ComicId); // Lấy tên truyện

            if (userIds.Any() && comic != null)
            {
                var notifications = new List<Notification>();
                foreach (var uid in userIds)
                {
                    notifications.Add(new Notification
                    {
                        UserId = uid,
                        Message = $"Truyện '{comic.Title}' vừa có chương mới: {entity.Title}",
                        Url = $"/comic/{comic.Slug}", // Link đến truyện
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
            return _mapper.Map<ChapterDTO>(entity);
        }

        public override async Task<ChapterDTO> UpdateAsync(int id, ChapterUpdateDTO dto)
        {
            var result = await base.UpdateAsync(id, dto); // Base đã gọi CommitAsync

            _cache.Remove($"chapter_id_{id}");

            return result;
        }

        public override async Task<bool> DeleteAsync(int id, bool hardDelete = false)
        {
            var result = await base.DeleteAsync(id, hardDelete); // Base đã gọi CommitAsync
            if (result)
            {
                _cache.Remove($"chapter_id_{id}");
            }
            return result;
        }
    }
}