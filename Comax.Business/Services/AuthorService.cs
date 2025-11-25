using AutoMapper;
using Comax.Business.Services.Interfaces;
using Comax.Common.DTOs.Author;
using Comax.Data.Entities;
using Comax.Data.Repositories.Interfaces;
using Comax.Business.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Comax.Common.DTOs.Pagination;

namespace Comax.Business.Services
{
    // CẬP NHẬT: Kế thừa BaseService và triển khai IAuthorService
    public class AuthorService : BaseService<Author, AuthorDTO, AuthorCreateDTO, AuthorUpdateDTO>, IAuthorService
    {
        private readonly IAuthorRepository _repo;
        private readonly IMapper _mapper;

        public AuthorService(IAuthorRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // TẤT CẢ các phương thức CRUD (Create, Update, Delete, GetAll, GetById, GetAllPagedAsync) 
        // ĐỀU ĐƯỢC KẾ THỪA TỪ BASESERVICE. 
        // Xóa các implementation bị trùng lặp trong file gốc.
    }
}