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
            // Author
            //CreateMap<Author, AuthorDTO>();
            //CreateMap<AuthorCreateDTO, Author>();
            //CreateMap<AuthorUpdateDTO, Author>();

            // Category
            CreateMap<Category, CategoryDTO>();
            CreateMap<CategoryCreateDTO, Category>();
            CreateMap<CategoryUpdateDTO, Category>();

            // Chapter
            CreateMap<Chapter, ChapterDTO>();
            CreateMap<ChapterCreateDTO, Chapter>();
            CreateMap<ChapterUpdateDTO, Chapter>();

            // Comic
            CreateMap<Comic, ComicDTO>();
            CreateMap<ComicCreateDTO, Comic>();
            CreateMap<ComicUpdateDTO, Comic>();

            // User
            CreateMap<User, UserDTO>();
            CreateMap<UserCreateDTO, User>();
            CreateMap<UserUpdateDTO, User>();
        }
    }
}
