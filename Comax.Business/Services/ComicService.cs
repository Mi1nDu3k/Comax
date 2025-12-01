using AutoMapper;
using Comax.Business.Interfaces;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Comic;
using Comax.Common.DTOs.Pagination;
using Comax.Common.Helpers;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Comax.Business.Services
{
    
    public class ComicService : BaseService<Comic, ComicDTO, ComicCreateDTO, ComicUpdateDTO>, IComicService
    {
        private readonly IComicRepository _repo;
        private readonly IMapper _mapper;

        public ComicService(IComicRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<ComicDTO?> GetBySlugAsync(string slug)
        {
            var comic = await _repo.GetBySlugAsync(slug);
            if (comic == null) return null;
            return _mapper.Map<ComicDTO>(comic);
        }

        // Override CreateAsync để tự động tạo Slug
        public override async Task<ComicDTO> CreateAsync(ComicCreateDTO dto)
        {
            // Tạo slug chuẩn từ Title (Tiếng Việt -> Slug)
            string slug = SlugHelper.GenerateSlug(dto.Title);

            // Kiểm tra trùng lặp trong DB
            // Nếu trùng, thêm số đếm vào đuôi: "conan" -> "conan-1" -> "conan-2"
            string originalSlug = slug;
            int count = 0;

            while ((await _repo.GetBySlugAsync(slug)) != null)
            {
                count++;
                slug = $"{originalSlug}-{count}";
            }

            //Map DTO sang Entity và gán Slug
            var entity = _mapper.Map<Comic>(dto);
            entity.Slug = slug;

            // Lưu vào DB
            await _repo.AddAsync(entity);

            //Trả về DTO
            return _mapper.Map<ComicDTO>(entity);
        }


        public async Task<IEnumerable<ComicDTO>> SearchByTitleAsync(string title)
        {
            var filteredEntities = await _repo.SearchByTitleAsync(title);
            return _mapper.Map<IEnumerable<ComicDTO>>(filteredEntities);
        }
        public async Task IncreaseViewCountAsync(int id)
        {
            var comic = await _repo.GetByIdAsync(id);
            if (comic != null)
            {
                comic.ViewCount++;
                await _repo.UpdateAsync(comic);
            }
        }


    }
}