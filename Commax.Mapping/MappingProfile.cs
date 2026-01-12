using AutoMapper;
using Comax.Common.DTOs;
using Comax.Common.DTOs.Auth;
using Comax.Common.DTOs.Author;
using Comax.Common.DTOs.Category;
using Comax.Common.DTOs.Chapter;
using Comax.Common.DTOs.Comic;
using Comax.Common.DTOs.Comment;
using Comax.Common.DTOs.History;
using Comax.Common.DTOs.Notification;
using Comax.Common.DTOs.Page;
using Comax.Common.DTOs.Rating;
using Comax.Common.DTOs.User;
using Comax.Data.Entities;

namespace Comax.Mapping
{
    public class MappingProfile : Profile
    {
        string baseUrl = "http://localhost:9000/comics-bucket";
        public MappingProfile()
        {
            // --- 1. MAPPING USER (Quan trọng nhất) ---
            CreateMap<User, UserDTO>()
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.Avatar) ? $"{baseUrl}/{src.Avatar}" : src.Avatar));
            CreateMap<RegisterDTO, User>();
            CreateMap<UserUpdateDTO, User>()
                // Bỏ qua các trường không được phép tự cập nhật hoặc cập nhật thủ công
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
              
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // --- 2. MAPPING COMIC ---
            CreateMap<Comic, ComicDTO>()
     
     .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src =>
         src.Author != null ? src.Author.Name : "N/A"))

  
     .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.CoverImage) ? $"{baseUrl}/{src.CoverImage}" : src.CoverImage))

   
     .ForMember(dest => dest.CategoryNames, opt => opt.MapFrom(src =>
         src.ComicCategories != null
             ? src.ComicCategories.Where(cc => cc.Category != null).Select(cc => cc.Category.Name).ToList()
             : new List<string>()))

   
     .ForMember(dest => dest.CategoryIds, opt => opt.MapFrom(src =>
         src.ComicCategories.Select(cc => cc.CategoryId).ToList()))

     
     .ForMember(dest => dest.Rating, opt => opt.MapFrom(src =>
         src.Ratings != null && src.Ratings.Any() ? src.Ratings.Average(r => r.Score) : 0))

 
     .ForMember(dest => dest.LatestChapterNumber, opt => opt.MapFrom(src =>
         src.Chapters != null && src.Chapters.Any()
             ? src.Chapters.Max(c => c.ChapterNumber)
             : (int?)null))


     .ForMember(dest => dest.LatestChapterDate, opt => opt.MapFrom(src =>
         src.Chapters != null && src.Chapters.Any()
             ? src.Chapters.Max(c => (DateTime?)c.PublishDate) ?? src.CreatedAt
             : src.CreatedAt));
            CreateMap<ComicCreateDTO, Comic>();

            CreateMap<ComicUpdateDTO, Comic>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.ComicCategories, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // --- 3. MAPPING CHAPTER ---
            CreateMap<Chapter, ChapterDTO>()
                .ForMember(dest => dest.Pages, opt => opt.MapFrom(src => src.Pages));

            CreateMap<ChapterCreateDTO, Chapter>()
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.ChapterNumber))
                .ForMember(dest => dest.Slug, opt => opt.Ignore());

            CreateMap<ChapterUpdateDTO, Chapter>()
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.ChapterNumber))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            // --- 4. MAPPING PAGE (Xử lý URL Minio Local) ---
            CreateMap<Page, PageDTO>()
               .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.ImageUrl)
                        ? $"{baseUrl}/{src.ImageUrl}" 
                        : src.ImageUrl));

            // --- 5. MAPPING CÁC THỰC THỂ KHÁC ---
            CreateMap<Author, AuthorDTO>();
            CreateMap<AuthorCreateDTO, Author>();
            CreateMap<AuthorUpdateDTO, Author>();

            CreateMap<Category, CategoryDTO>();
            CreateMap<CategoryCreateDTO, Category>();
            CreateMap<CategoryUpdateDTO, Category>();

            CreateMap<Comment, CommentDTO>()
     .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : "Ẩn danh"))
     .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src =>
         src.User != null
             ? (!string.IsNullOrEmpty(src.User.Avatar) ? $"{baseUrl}/{src.User.Avatar}" : src.User.Avatar)
             : null))

     .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));
            CreateMap<CommentCreateDTO, Comment>();
            CreateMap<CommentUpdateDTO, Comment>();


            CreateMap<Notification, NotificationDTO>()
              .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender != null ? src.Sender.Username : "Hệ thống"))
              .ForMember(dest => dest.SenderAvatar, opt => opt.MapFrom(src => src.Sender != null ? src.Sender.Avatar : null));

            CreateMap<History, HistoryDTO>()
     .ForMember(dest => dest.ComicTitle, opt => opt.MapFrom(src => src.Comic.Title))
     .ForMember(dest => dest.ComicImage, opt => opt.MapFrom(src => src.Comic.CoverImage))
     .ForMember(dest => dest.ChapterNumber, opt => opt.MapFrom(src => src.Chapter.ChapterNumber));

        }
    }
}