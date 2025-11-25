using AutoMapper;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Comic;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Comax.Business.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Comax.Common.DTOs.Pagination;
using System.Linq;

namespace Comax.Business.Services
{
    // CẬP NHẬT: Kế thừa BaseService và triển khai IComicService
    public class ComicService : BaseService<Comic, ComicDTO, ComicCreateDTO, ComicUpdateDTO>, IComicService
    {
        private readonly IComicRepository _repo;
        private readonly IMapper _mapper;

        public ComicService(IComicRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // GIỮ LẠI phương thức độc nhất: SearchByTitleAsync
        public async Task<IEnumerable<ComicDTO>> SearchByTitleAsync(string title)
        {
            var filteredEntities = await _repo.SearchByTitleAsync(title);
            return _mapper.Map<IEnumerable<ComicDTO>>(filteredEntities);
        }

        // TẤT CẢ các phương thức CRUD còn lại được KẾ THỪA.
    }
}