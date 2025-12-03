using Comax.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Comax.Common.Helpers;
using Microsoft.Extensions.Configuration;

namespace Comax.Data
{
    public static class DbSeeder
    {
        private const string FIXED_PASSWORD = "Comax@123";

        public static void Seed(ComaxDbContext context, IConfiguration configuration = null)
        {
            // --- 0. CHUẨN BỊ ẢNH ---
            string comicCoverBaseUrl = "https://placehold.co/200x300.png";

            if (configuration != null)
            {
                string endpoint = configuration["Minio:Endpoint"];
                string bucketName = configuration["Minio:BucketName"];
                if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(bucketName))
                {
                    bool useSSL = bool.Parse(configuration["Minio:UseSSL"] ?? "false");
                    string protocol = useSSL ? "https" : "http";
                    comicCoverBaseUrl = $"{protocol}://{endpoint}/{bucketName}/seed";
                }
            }

            // --- 1. SEED ROLES ---
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "User" },
                    new Role { Name = "VipUser" }
                );
                context.SaveChanges();
            }

            // --- 2. SEED USERS (CẬP NHẬT PHẦN BAN) ---
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.First(r => r.Name == "Admin");
                var userRole = context.Roles.First(r => r.Name == "User");
                var vipRole = context.Roles.First(r => r.Name == "VipUser");

                var commonHash = PasswordHelper.HashPassword(FIXED_PASSWORD);

                var users = new List<User>
                {
                    // Admin
                    new User { Username = "admin", Email = "admin@comax.com", PasswordHash = commonHash, RoleId = adminRole.Id, IsVip = true, IsBanned = false, CreatedAt = DateTime.UtcNow },
                    
                    // VIP User
                    new User { Username = "vipuser", Email = "vip@comax.com", PasswordHash = commonHash, RoleId = userRole.Id, IsVip = true, IsBanned = false, CreatedAt = DateTime.UtcNow },

                    // --- THÊM: User bị khóa cứng để test ---
                    new User {
                        Username = "banned_guy",
                        Email = "banned@comax.com",
                        PasswordHash = commonHash,
                        RoleId = userRole.Id,
                        IsVip = false,
                        IsBanned = true, // <--- BỊ KHÓA
                        CreatedAt = DateTime.UtcNow
                    }
                };

                // User thường (Random một số bị khóa)
                for (int i = 1; i <= 20; i++)
                {
                    // Logic: Cứ user số 7, 14... thì bị khóa
                    bool isBanned = (i % 7 == 0);

                    users.Add(new User
                    {
                        Username = $"user{i}",
                        Email = $"user{i}@comax.com",
                        PasswordHash = commonHash,
                        RoleId = userRole.Id,
                        IsVip = false,
                        IsBanned = isBanned, // Set trạng thái khóa
                        CreatedAt = DateTime.UtcNow
                    });
                }
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // --- 3. SEED AUTHORS & CATEGORIES (Giữ nguyên) ---
            if (!context.Authors.Any())
            {
                context.Authors.AddRange(new Author { Name = "Oda" }, new Author { Name = "Toriyama" }, new Author { Name = "Kishimoto" });
                context.SaveChanges();
            }

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Action", Slug = "action" },
                    new Category { Name = "Adventure", Slug = "adventure" },
                    new Category { Name = "Comedy", Slug = "comedy" },
                    new Category { Name = "Fantasy", Slug = "fantasy" }
                );
                context.SaveChanges();
            }

            // --- 4. SEED COMICS & CHAPTERS (Giữ nguyên) ---
            if (!context.Comics.Any())
            {
                var authors = context.Authors.ToList();
                var categories = context.Categories.ToList();
                var random = new Random();
                var comics = new List<Comic>();

                for (int i = 1; i <= 10; i++)
                {
                    string title = $"Comic Volume {i}";
                    string slug = GenerateSlug(title);

                    string coverUrl = (!string.IsNullOrEmpty(comicCoverBaseUrl) && !comicCoverBaseUrl.Contains("placehold.co"))
                        ? $"{comicCoverBaseUrl}/comic-{i}.jpg"
                        : comicCoverBaseUrl;

                    var comic = new Comic
                    {
                        Title = title,
                        Slug = slug,
                        Description = $"Mô tả hấp dẫn cho truyện số {i}...",
                        CoverImage = coverUrl,
                        Status = i % 3 == 0 ? "Completed" : "Ongoing",
                        Rating = (float)(random.Next(35, 50) / 10.0),
                        AuthorId = authors[random.Next(authors.Count)].Id,
                        ViewCount = random.Next(100, 50000),
                        CreatedAt = DateTime.UtcNow.AddMonths(-random.Next(1, 24))
                    };
                    comics.Add(comic);
                }
                context.Comics.AddRange(comics);
                context.SaveChanges();

                foreach (var comic in comics)
                {
                    var randomCategory = categories[random.Next(categories.Count)];
                    context.ComicCategories.Add(new ComicCategory { ComicId = comic.Id, CategoryId = randomCategory.Id });
                }
                context.SaveChanges();

                foreach (var comic in comics)
                {
                    var chapters = new List<Chapter>();
                    for (int ch = 1; ch <= 20; ch++)
                    {
                        bool isVipChapter = ch > 18;
                        string titlePrefix = isVipChapter ? "[VIP] " : "";
                        DateTime pubDate = isVipChapter ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddDays(-10);

                        chapters.Add(new Chapter
                        {
                            Title = $"{titlePrefix}Chapter {ch}",
                            Slug = GenerateSlug($"{comic.Slug}-chapter-{ch}"),
                            Order = ch,
                            Content = "Nội dung truyện demo...",
                            ComicId = comic.Id,
                            PublishDate = pubDate
                        });
                    }
                    context.Chapters.AddRange(chapters);
                }
                context.SaveChanges();
            }

            // --- 5. SEED RATINGS (Giữ nguyên) ---
            if (!context.Ratings.Any())
            {
                var allUsers = context.Users.ToList();
                var allComics = context.Comics.ToList();
                var random = new Random();
                var ratings = new List<Rating>();
                var comments = new List<Comment>();

                for (int i = 0; i < 50; i++)
                {
                    var user = allUsers[random.Next(allUsers.Count)];
                    var comic = allComics[random.Next(allComics.Count)];

                    ratings.Add(new Rating
                    {
                        UserId = user.Id,
                        ComicId = comic.Id,
                        Score = random.Next(3, 6),
                        Comment = "Truyện rất hay!",
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30))
                    });

                    comments.Add(new Comment
                    {
                        UserId = user.Id,
                        ComicId = comic.Id,
                        Content = "Hóng chap mới!",
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 10))
                    });
                }
                context.Ratings.AddRange(ratings);
                context.Comments.AddRange(comments);
                context.SaveChanges();
            }
        }

        private static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return "";
            string str = phrase.ToLowerInvariant();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}