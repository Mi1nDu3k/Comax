using Comax.Data.Entities;
using Comax.Common.Helpers;
using System.Collections.Generic; // Đảm bảo có
using System.Linq; // Đảm bảo có
using Microsoft.EntityFrameworkCore; // Đảm bảo có

namespace Comax.Data
{
    public static class DataSeeder
    {
        public static void Seed(ComaxDbContext context)
        {
            // 1. Seed Roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "User" }
                );
                context.SaveChanges();
            }

            // 2. Seed Users
            // Lấy Role trước khi Seed User
            var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");
            var userRole = context.Roles.FirstOrDefault(r => r.Name == "User");

            if (adminRole != null && userRole != null && !context.Users.Any())
            {
                var users = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        Email = "admin@comax.com",
                        PasswordHash = PasswordHelper.HashPassword("Admin@123"),
                        RoleId = adminRole.Id
                    },
                    new User
                    {
                        Username = "user01",
                        Email = "user01@comax.com",
                        PasswordHash = PasswordHelper.HashPassword("User@123"),
                        RoleId = userRole.Id
                    },
                    new User
                    {
                        Username = "user02",
                        Email = "user02@comax.com",
                        PasswordHash = PasswordHelper.HashPassword("User@123"),
                        RoleId = userRole.Id
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }

            // 3. Seed Authors
            if (!context.Authors.Any())
            {
                context.Authors.AddRange(
                    new Author { Name = "Eiichiro Oda" },
                    new Author { Name = "Akira Toriyama" },
                    new Author { Name = "Fujiko F. Fujio" },
                    new Author { Name = "Author Demo" }
                );
                context.SaveChanges();
            }

            // 4. Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Action" },
                    new Category { Name = "Adventure" },
                    new Category { Name = "Comedy" },
                    new Category { Name = "Sci-Fi" },
                    new Category { Name = "Fantasy" },
                    new Category { Name = "Slice of Life" }
                );
                context.SaveChanges();
            }

            // 5. Seed Comics
            var oda = context.Authors.FirstOrDefault(a => a.Name == "Eiichiro Oda");
            var toriyama = context.Authors.FirstOrDefault(a => a.Name == "Akira Toriyama");
            var fujio = context.Authors.FirstOrDefault(a => a.Name == "Fujiko F. Fujio");

            if (oda != null && toriyama != null && fujio != null && !context.Comics.Any())
            {
                context.Comics.AddRange(
                    new Comic
                    {
                        Title = "One Piece",
                        Description = "Hành trình trở thành Vua Hải Tặc của Luffy.",
                        AuthorId = oda.Id,
                        CreatedAt = DateTime.UtcNow.AddMonths(-12)
                    },
                    new Comic
                    {
                        Title = "Dragon Ball",
                        Description = "Khỉ con Son Goku và hành trình tìm ngọc rồng.",
                        AuthorId = toriyama.Id,
                        CreatedAt = DateTime.UtcNow.AddMonths(-24)
                    },
                    new Comic
                    {
                        Title = "Doraemon",
                        Description = "Chú mèo máy đến từ tương lai.",
                        AuthorId = fujio.Id,
                        CreatedAt = DateTime.UtcNow.AddMonths(-36)
                    }
                );
                context.SaveChanges();
            }

            // 6. Seed ComicCategories (Many-to-Many)
            // Lấy entities bằng FirstOrDefault() để đảm bảo an toàn
            var onePiece = context.Comics.FirstOrDefault(c => c.Title == "One Piece");
            var dragonBall = context.Comics.FirstOrDefault(c => c.Title == "Dragon Ball");
            var doraemon = context.Comics.FirstOrDefault(c => c.Title == "Doraemon");

            var action = context.Categories.FirstOrDefault(c => c.Name == "Action");
            var adventure = context.Categories.FirstOrDefault(c => c.Name == "Adventure");
            var comedy = context.Categories.FirstOrDefault(c => c.Name == "Comedy");
            var scifi = context.Categories.FirstOrDefault(c => c.Name == "Sci-Fi");

            if (onePiece != null && dragonBall != null && doraemon != null &&
                action != null && adventure != null && comedy != null && scifi != null &&
                !context.ComicCategories.Any())
            {
                context.ComicCategories.AddRange(
                    new ComicCategory { ComicId = onePiece.Id, CategoryId = action.Id },
                    new ComicCategory { ComicId = onePiece.Id, CategoryId = adventure.Id },
                    new ComicCategory { ComicId = dragonBall.Id, CategoryId = action.Id },
                    new ComicCategory { ComicId = dragonBall.Id, CategoryId = scifi.Id },
                    new ComicCategory { ComicId = doraemon.Id, CategoryId = comedy.Id },
                    new ComicCategory { ComicId = doraemon.Id, CategoryId = scifi.Id }
                );
                context.SaveChanges();
            }

            // 7. Seed Chapters
            if (onePiece != null && dragonBall != null && !context.Chapters.Any())
            {
                context.Chapters.AddRange(
                    // One Piece Chapters
                    new Chapter { Title = "Romance Dawn", Order = 1, Content = "Luffy ra khơi...", ComicId = onePiece.Id },
                    new Chapter { Title = "Gặp gỡ Zoro", Order = 2, Content = "Thợ săn hải tặc...", ComicId = onePiece.Id },
                    new Chapter { Title = "Morgan tay rìu", Order = 3, Content = "Trận chiến tại căn cứ hải quân...", ComicId = onePiece.Id },

                    // Dragon Ball Chapters
                    new Chapter { Title = "Bulma và Son Goku", Order = 1, Content = "Cuộc gặp gỡ định mệnh...", ComicId = dragonBall.Id },
                    new Chapter { Title = "Hành trình bắt đầu", Order = 2, Content = "Đi tìm ngọc rồng...", ComicId = dragonBall.Id }
                );
                context.SaveChanges();
            }

            // 8. Seed Ratings
            if (!context.Ratings.Any())
            {
                // Sửa lỗi: Dùng FirstOrDefault() và kiểm tra null
                var user1 = context.Users.FirstOrDefault(u => u.Username == "user01");
                var user2 = context.Users.FirstOrDefault(u => u.Username == "user02");
                var onePieceComic = context.Comics.FirstOrDefault(c => c.Title == "One Piece");
                var doraemonComic = context.Comics.FirstOrDefault(c => c.Title == "Doraemon");

                if (user1 != null && user2 != null && onePieceComic != null && doraemonComic != null)
                {
                    context.Ratings.AddRange(
                        new Rating { ComicId = onePieceComic.Id, UserId = user1.Id, Score = 5, Comment = "Tuyệt phẩm!", CreatedAt = DateTime.UtcNow },
                        new Rating { ComicId = onePieceComic.Id, UserId = user2.Id, Score = 4, Comment = "Rất hay nhưng hơi dài.", CreatedAt = DateTime.UtcNow },
                        new Rating { ComicId = doraemonComic.Id, UserId = user1.Id, Score = 5, Comment = "Tuổi thơ của tôi.", CreatedAt = DateTime.UtcNow }
                    );
                    context.SaveChanges();
                }
            }

            // 9. Seed Comments
            if (!context.Comments.Any())
            {
                // Sửa lỗi: Dùng FirstOrDefault() và kiểm tra null
                var user1 = context.Users.FirstOrDefault(u => u.Username == "user01");
                var admin = context.Users.FirstOrDefault(u => u.Username == "admin");
                var onePieceComic = context.Comics.FirstOrDefault(c => c.Title == "One Piece");

                if (user1 != null && admin != null && onePieceComic != null)
                {
                    context.Comments.AddRange(
                        new Comment { ComicId = onePieceComic.Id, UserId = user1.Id, Content = "Hóng chap mới quá ad ơi!", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                        new Comment { ComicId = onePieceComic.Id, UserId = admin.Id, Content = "Sẽ sớm có nhé bạn.", CreatedAt = DateTime.UtcNow.AddDays(-1) }
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}