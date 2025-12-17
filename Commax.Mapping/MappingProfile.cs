using AutoMapper;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Author;
using Comax.Common.DTOs.Category;
using Comax.Common.DTOs.Chapter;
using Comax.Common.DTOs.Comic;
using Comax.Common.DTOs.Comment;
using Comax.Common.DTOs.Page;
using Comax.Common.DTOs.Rating;
using Comax.Common.DTOs.User;
using Comax.Data.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Comax.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Page, PageDTO>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
        !string.IsNullOrEmpty(src.ImageUrl)
            ? src.ImageUrl.Replace("http://minio:9000", "http://localhost:9000")
            : src.ImageUrl
    ));
            CreateMap<Author, AuthorDTO>();
            CreateMap<AuthorCreateDTO, Author>();
            CreateMap<AuthorUpdateDTO, Author>();

            // Category
            CreateMap<Category, CategoryDTO>();
            CreateMap<CategoryCreateDTO, Category>();
            CreateMap<CategoryUpdateDTO, Category>();

            // Chapter
            CreateMap<Chapter, ChapterDTO>()
             .ForMember(dest => dest.Pages, opt => opt.MapFrom(src => src.Pages));

            // 2. CreateDTO (ChapterNumber) -> Entity (Order)
            CreateMap<ChapterCreateDTO, Chapter>()
                // Lỗi cũ là do gọi src.number, phải sửa thành src.ChapterNumber
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.ChapterNumber))
                .ForMember(dest => dest.Slug, opt => opt.Ignore());

            // 3. UpdateDTO (ChapterNumber) -> Entity (Order)
            CreateMap<ChapterUpdateDTO, Chapter>()
              
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.ChapterNumber))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Comic
            CreateMap<Comic, ComicDTO>()
                // 1. Map CoverImage -> ThumbnailUrl
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.CoverImage))
                // 2. Map Author.Name -> AuthorName (Kiểm tra null an toàn)
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Name : "N/A"))
                // 3. Map Categories
                .ForMember(dest => dest.CategoryIds, opt => opt.MapFrom(src => src.ComicCategories.Select(cc => cc.CategoryId)));
            CreateMap<ComicCreateDTO, Comic>();

            CreateMap<ComicUpdateDTO, Comic>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.Author, opt => opt.Ignore())         
        .ForMember(dest => dest.ComicCategories, opt => opt.Ignore())
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // User
            CreateMap<User, UserDTO>();
            CreateMap<UserCreateDTO, User>();
            CreateMap<UserUpdateDTO, User>();
        //rating and comment mappings can be added here when needed
                CreateMap<Rating,RatingCreateDTO>();
                CreateMap<Rating,RatingUpdateDTO>();
            
            
            CreateMap<Comment, CommentDTO>()
   .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username)); // Map tên user
            CreateMap<CommentCreateDTO, Comment>();
            CreateMap<CommentUpdateDTO, Comment>();




        }
    }
}
