using AutoMapper;
using Comax.Common.DTOs;
using Comax.Data.Entities;

namespace Comax.API.Mapping
{
    public class ComaxMappingProfile : Profile
    {
        public ComaxMappingProfile()
        {
            CreateMap<Author, AuthorDTO>().ReverseMap();
            CreateMap<AuthorCreateDTO, Author>();
            CreateMap<AuthorUpdateDTO, Author>();
            CreateMap<Category, CategoryDTO>().ReverseMap();
            CreateMap<ComicCategory, CategoryDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<Comic, ComicDTO>().ReverseMap();
            CreateMap<ComicCreateDTO, Comic>();
            CreateMap<Chapter, ChapterDTO>().ReverseMap();
            CreateMap<ChapterCreateDTO, Chapter>();
            CreateMap<ChapterUpdateDTO, Chapter>();
            CreateMap<CategoryCreateDTO, Category>();
            CreateMap<CategoryUpdateDTO, Category>();


        }
    }
}