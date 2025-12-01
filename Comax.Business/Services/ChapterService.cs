using AutoMapper;
using Comax.Business.Services; // Namespace chứa BaseService
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Chapter;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace Comax.Business.Services
{
    public class ChapterService : BaseService<Chapter, ChapterDTO, ChapterCreateDTO, ChapterUpdateDTO>, IChapterService
    {
        private readonly IChapterRepository _repo;
        private readonly IComicRepository _comicRepo; 
        private readonly IMapper _mapper;

        public ChapterService(IChapterRepository repo, IComicRepository comicRepo, IMapper mapper)
            : base(repo, mapper)
        {
            _repo = repo;
            _comicRepo = comicRepo; 
            _mapper = mapper;
        }

        public async Task<ChapterDTO?> GetChapterBySlugsAsync(string comicSlug, string chapterSlug)
        {
      
            var comic = await _comicRepo.GetBySlugAsync(comicSlug);


            if (comic == null) return null;

            var chapter = await _repo.GetByComicIdAndSlugAsync(comic.Id, chapterSlug);

            if (chapter == null) return null;

            return _mapper.Map<ChapterDTO>(chapter);
        }
        public override async Task<ChapterDTO> CreateAsync(ChapterCreateDTO dto)
        {
            // Tạo slug từ Title (VD: "Chương 1" -> "chuong-1")
            string slug = SlugHelper.GenerateSlug(dto.Title);

            // Kiểm tra trùng lặp trong phạm vi truyện đó
            var existingChapter = await _repo.GetByComicIdAndSlugAsync(dto.ComicId, slug);

            if (existingChapter != null)
            {
                // Nếu trùng, có thể báo lỗi hoặc tự thêm số (tùy logic của bạn)
                throw new Exception($"Chapter slug '{slug}' already exists in this comic.");
            }

            var entity = _mapper.Map<Chapter>(dto);
            entity.Slug = slug;

            await _repo.AddAsync(entity);
            return _mapper.Map<ChapterDTO>(entity);
        }
    }
}