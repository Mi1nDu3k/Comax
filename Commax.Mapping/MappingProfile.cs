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
using System.Linq;

namespace Comax.Mapping
{
    public class MappingProfile : Profile
    {
        // Lưu ý: Tốt nhất nên lấy từ IConfiguration, nhưng để hardcode tạm cũng được
        string baseUrl = "http://localhost:9000/comics-bucket";

        public MappingProfile()
        {
            // --- 1. USER MAPPING ---
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.Avatar) && !src.Avatar.StartsWith("http")
                        ? $"{baseUrl}/{src.Avatar}"
                        : src.Avatar));

            CreateMap<RegisterDTO, User>();

            CreateMap<UserUpdateDTO, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.RoleId, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // --- 2. COMIC MAPPING ---
            CreateMap<Comic, ComicDTO>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src =>
                    src.Author != null ? src.Author.Name : "Đang cập nhật"))

                // Xử lý ảnh bìa (Thumbnail)
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.CoverImage) && !src.CoverImage.StartsWith("http")
                        ? $"{baseUrl}/{src.CoverImage}"
                        : src.CoverImage))

                // Map danh sách tên thể loại
                .ForMember(dest => dest.CategoryNames, opt => opt.MapFrom(src =>
                    src.ComicCategories != null
                        ? src.ComicCategories.Where(cc => cc.Category != null).Select(cc => cc.Category.Name).ToList()
                        : new List<string>()))

                // Map danh sách ID thể loại
                .ForMember(dest => dest.CategoryIds, opt => opt.MapFrom(src =>
                    src.ComicCategories.Select(cc => cc.CategoryId).ToList()))

                // Tính điểm đánh giá trung bình
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src =>
                    src.Ratings != null && src.Ratings.Any() ? Math.Round(src.Ratings.Average(r => r.Score), 1) : 0))

                // Lấy chương mới nhất (Sửa logic để tránh lỗi null)
                .ForMember(dest => dest.LatestChapterNumber, opt => opt.MapFrom(src =>
                    src.Chapters != null && src.Chapters.Any()
                        ? src.Chapters.Max(c => c.ChapterNumber)
                        : (float?)0)) // Ép kiểu float? hoặc int? tùy DTO của bạn

                // Lấy ngày cập nhật mới nhất
                .ForMember(dest => dest.LatestChapterDate, opt => opt.MapFrom(src =>
                    src.Chapters != null && src.Chapters.Any()
                        ? src.Chapters.Max(c => (DateTime?)c.PublishDate) ?? src.UpdatedAt
                        : src.UpdatedAt));

            CreateMap<ComicCreateDTO, Comic>();

            CreateMap<ComicUpdateDTO, Comic>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Author, opt => opt.Ignore())
                .ForMember(dest => dest.ComicCategories, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // --- 3. CHAPTER MAPPING ---
            CreateMap<Chapter, ChapterDTO>()
                .ForMember(dest => dest.Pages, opt => opt.MapFrom(src => src.Pages));

            CreateMap<ChapterCreateDTO, Chapter>()
                // Giả sử DTO dùng tên 'ChapterNumber' và Entity dùng 'ChapterNumber' (hoặc Order)
                .ForMember(dest => dest.ChapterNumber, opt => opt.MapFrom(src => src.ChapterNumber))
                .ForMember(dest => dest.Slug, opt => opt.Ignore());

            CreateMap<ChapterUpdateDTO, Chapter>()
                .ForMember(dest => dest.ChapterNumber, opt => opt.MapFrom(src => src.ChapterNumber))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // --- 4. PAGE MAPPING ---
            CreateMap<Page, PageDTO>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.ImageUrl) && !src.ImageUrl.StartsWith("http")
                        ? $"{baseUrl}/{src.ImageUrl}"
                        : src.ImageUrl));

            // --- 5. OTHER MAPPINGS ---
            CreateMap<Author, AuthorDTO>();
            CreateMap<AuthorCreateDTO, Author>();
            CreateMap<AuthorUpdateDTO, Author>();

            CreateMap<Category, CategoryDTO>();
            CreateMap<CategoryCreateDTO, Category>();
            CreateMap<CategoryUpdateDTO, Category>();

            CreateMap<Comment, CommentDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : "Ẩn danh"))
                .ForMember(dest => dest.UserAvatar, opt => opt.MapFrom(src =>
                    src.User != null && !string.IsNullOrEmpty(src.User.Avatar) && !src.User.Avatar.StartsWith("http")
                        ? $"{baseUrl}/{src.User.Avatar}"
                        : (src.User != null ? src.User.Avatar : null)))
                .ForMember(dest => dest.Replies, opt => opt.MapFrom(src => src.Replies));

            CreateMap<CommentCreateDTO, Comment>();
            CreateMap<CommentUpdateDTO, Comment>();

            CreateMap<Notification, NotificationDTO>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender != null ? src.Sender.Username : "Hệ thống"))
                .ForMember(dest => dest.SenderAvatar, opt => opt.MapFrom(src =>
                    src.Sender != null && !string.IsNullOrEmpty(src.Sender.Avatar) && !src.Sender.Avatar.StartsWith("http")
                    ? $"{baseUrl}/{src.Sender.Avatar}"
                    : null));

            CreateMap<History, HistoryDTO>()
                .ForMember(dest => dest.ComicTitle, opt => opt.MapFrom(src => src.Comic != null ? src.Comic.Title : "Unknown"))
                // Bổ sung xử lý ảnh cho History (bạn đang thiếu cái này)
                .ForMember(dest => dest.ComicImage, opt => opt.MapFrom(src =>
                    src.Comic != null && !string.IsNullOrEmpty(src.Comic.CoverImage) && !src.Comic.CoverImage.StartsWith("http")
                    ? $"{baseUrl}/{src.Comic.CoverImage}"
                    : (src.Comic != null ? src.Comic.CoverImage : null)))
                .ForMember(dest => dest.ChapterNumber, opt => opt.MapFrom(src => src.Chapter != null ? src.Chapter.ChapterNumber : 0));
        }
    }
}