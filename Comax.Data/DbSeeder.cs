using Comax.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Comax.Common.Helpers;

namespace Comax.Data
{
    public static class DbSeeder
    {
        private const string FIXED_PASSWORD = "Comax@123";

        public static void Seed(ComaxDbContext context)
        {
            // 1. Seed Roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "User" },
                    new Role { Name = "VipUser" }
                );
                context.SaveChanges();
            }

            // 2. Seed Users
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.First(r => r.Name == "Admin");
                var userRole = context.Roles.First(r => r.Name == "User");
                var vipRole = context.Roles.First(r => r.Name == "VipUser");

                var commonHash = PasswordHelper.HashPassword(FIXED_PASSWORD);

                var users = new List<User>
                {
                    new User { Username = "admin", Email = "admin@comax.com", PasswordHash = commonHash, RoleId = adminRole.Id, IsVip = true },
                    new User { Username = "vipuser", Email = "vip@comax.com", PasswordHash = commonHash, RoleId = userRole.Id, IsVip = true }
                };

                for (int i = 1; i <= 20; i++)
                {
                    users.Add(new User
                    {
                        Username = $"user{i}",
                        Email = $"user{i}@comax.com",
                        PasswordHash = commonHash,
                        RoleId = userRole.Id,
                        IsVip = false
                    });
                }
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // 3. Seed Authors & Categories
            if (!context.Authors.Any())
            {
                context.Authors.AddRange(new Author { Name = "Oda" }, new Author { Name = "Toriyama" });
                context.SaveChanges();
            }

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Action", Slug = "action" },
                    new Category { Name = "Comedy", Slug = "comedy" }
                );
                context.SaveChanges();
            }

            // 4. Seed Comics (Đảm bảo có Slug, CoverImage, Status)
            if (!context.Comics.Any())
            {
                var authors = context.Authors.ToList();
                var random = new Random();
                var comics = new List<Comic>();

                for (int i = 1; i <= 5; i++)
                {
                    string title = $"Comic Volume {i}";

                    string slug = GenerateSlug(title);
                    if (string.IsNullOrEmpty(slug)) slug = $"comic-{i}";

                    var comic = new Comic
                    {
                        Title = title,
                        Slug = slug,
                        Description = "Mô tả truyện...",
                        CoverImage = "https://placehold.co/200x300.png",
                        Status = "Ongoing", // <-- ĐÃ SỬA: Thêm Status để tránh lỗi null
                        Rating = 4.5f,      // Thêm điểm đánh giá mặc định cho đẹp
                        AuthorId = authors[random.Next(authors.Count)].Id,
                        ViewCount = random.Next(100, 5000),
                        CreatedAt = DateTime.UtcNow
                    };
                    comics.Add(comic);
                }
                context.Comics.AddRange(comics);
                context.SaveChanges();

                // Seed Chapters
                foreach (var comic in comics)
                {
                    context.Chapters.Add(new Chapter
                    {
                        Title = "Chapter 1",
                        Slug = "chapter-1",
                        Order = 1,
                        Content = "Nội dung...",
                        ComicId = comic.Id,
                        PublishDate = DateTime.UtcNow.AddDays(-1)
                    });
                }
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