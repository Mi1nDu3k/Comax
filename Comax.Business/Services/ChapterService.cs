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
        private readonly IChapterRepository _chapterRepo; // Đổi tên để tránh nhầm với _repo của BaseService
        private readonly IComicRepository _comicRepo;
        private readonly IMemoryCache _cache;

        public ChapterService(
            IChapterRepository repo,
            IComicRepository comicRepo,
            IMapper mapper,
            IMemoryCache cache) : base(repo, mapper)
        {
            _chapterRepo = repo;
            _comicRepo = comicRepo;
            _cache = cache;
        }

        /// <summary>
        /// Lấy chi tiết Chapter theo ID (Cache)
        /// </summary>
        public override async Task<ChapterDTO?> GetByIdAsync(int id)
        {
            string key = $"chapter_id_{id}";
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(20);
                return await base.GetByIdAsync(id);
            });
        }

        /// <summary>
        /// Lấy Chapter theo Slug truyện và Slug chương (Dùng cho trang đọc)
        /// </summary>
        
        public async Task<ChapterDTO?> GetChapterBySlugsAsync(string comicSlug, string chapterSlug)
        {
            string key = $"chapter_read_{comicSlug}_{chapterSlug}";

            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(20);

                /// 1. Tìm Comic trước để lấy ID
                var comic = await _comicRepo.GetBySlugAsync(comicSlug);
                if (comic == null) return null;

                /// 2. Tìm Chapter dựa trên ComicId và ChapterSlug
                var chapter = await _chapterRepo.GetByComicIdAndSlugAsync(comic.Id, chapterSlug);
                if (chapter == null) return null;

                return _mapper.Map<ChapterDTO>(chapter);
            });
        }

        /// <summary>
        /// --- 2. WRITE (XÓA CACHE 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>& LOGIC SLUG) ---

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

         
             _cache.Remove($"chapters_of_comic_{dto.ComicId}");

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
    }
}