using Comax.Data.Entities;
using Comax.Common.Helpers;

namespace Comax.Data
{
    public static class DataSeeder
    {
        public static void Seed(ComaxDbContext context)
        {
            // Seed Roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "User" }
                );
                context.SaveChanges();
            }

            // Seed Users
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.First(r => r.Name == "Admin");

                context.Users.Add(new User
                {
                    Username = "admin",
                    Email = "admin@comax.com",
                    PasswordHash = PasswordHelper.HashPassword("Admin@123"),
                    RoleId = adminRole.Id
                });

                context.SaveChanges();
            }

            // Seed Authors
            if (!context.Authors.Any())
            {
                context.Authors.AddRange(
                    new Author { Name = "Author A" },
                    new Author { Name = "Author B" }
                );

                context.SaveChanges();
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Action" },
                    new Category { Name = "Comedy" },
                    new Category { Name = "Romance" }
                );

                context.SaveChanges();
            }

            // Seed Comics
            if (!context.Comics.Any())
            {
                var author = context.Authors.First();

                context.Comics.Add(new Comic
                {
                    Title = "Comic 1",
                    Description = "This is the first comic.",
                    AuthorId = author.Id,
                    CreatedAt = DateTime.Now
                });

                context.SaveChanges();
            }

            // Seed ComicCategories (many-to-many)
            if (!context.ComicCategories.Any())
            {
                var comic = context.Comics.First();
                var category = context.Categories.First();

                context.ComicCategories.Add(new ComicCategory
                {
                    ComicId = comic.Id,
                    CategoryId = category.Id
                });

                context.SaveChanges();
            }

            // Seed Chapters
            if (!context.Chapters.Any())
            {
                var comic = context.Comics.First();

                context.Chapters.Add(new Chapter
                {
                    Title = "Chapter 1",
                    Order = 1,
                    Content = "Content of chapter 1",
                    ComicId = comic.Id
                });

                context.SaveChanges();
            }
        }
    }
}
