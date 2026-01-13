using AutoMapper;
using Comax.Common.DTOs.Author;
using Comax.Common.DTOs.Category;
using Comax.Common.DTOs.Chapter;
using Comax.Common.DTOs.Comic;
using Comax.Common.DTOs.User;
using Comax.Data.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Comax.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ===== Author =====
            CreateMap<Author, AuthorDTO>();
            CreateMap<AuthorCreateDTO, Author>();
            CreateMap<AuthorUpdateDTO, Author>();

            // ===== Comic =====
            CreateMap<Comic, ComicDTO>()
                .ForMember(dest => dest.CategoryIds, opt => opt
                    .MapFrom(src => src.ComicCategories.Select(cc => cc.CategoryId)));
            CreateMap<ComicCreateDTO, Comic>();
            CreateMap<ComicUpdateDTO, Comic>();

            // ===== Chapter =====
            CreateMap<Chapter, ChapterDTO>();
            CreateMap<ChapterCreateDTO, Chapter>();
            CreateMap<ChapterUpdateDTO, Chapter>();

            // ===== Category =====
            CreateMap<Category, CategoryDTO>();
            CreateMap<CategoryCreateDTO, Category>();
            CreateMap<CategoryUpdateDTO, Category>();

            // ===== User =====
            CreateMap<User, UserDTO>();
            CreateMap<RegisterDTO, User>();
            CreateMap<UserUpdateDTO, User>();
        }
    }
}
