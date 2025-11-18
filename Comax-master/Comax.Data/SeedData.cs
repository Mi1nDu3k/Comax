using Comax.Data.Entities;

namespace Comax.Data
{
    public static class SeedData
    {
        public static void Initialize(ComaxDbContext context)
        {
            // Nếu đã có dữ liệu, không seed lại
            if (context.Authors.Any()) return;

            // ---------------------
            // 1. Seed Authors
            // ---------------------
            var authors = new List<Author>
            {
                new Author { Name = "Eiichiro Oda" },
                new Author { Name = "Masashi Kishimoto" },
                new Author { Name = "Tite Kubo" }
            };
            context.Authors.AddRange(authors);
            context.SaveChanges();

            // ---------------------
            // 2. Seed Comics
            // ---------------------
            var comics = new List<Comic>
            {
                new Comic { Title = "One Piece", Description = "Cuộc phiêu lưu của Luffy", AuthorId = authors[0].Id },
                new Comic { Title = "Naruto", Description = "Hành trình trở thành Hokage của Naruto", AuthorId = authors[1].Id },
                new Comic { Title = "Bleach", Description = "Câu chuyện của Ichigo Kurosaki", AuthorId = authors[2].Id }
            };
            context.Comics.AddRange(comics);
            context.SaveChanges();

            // ---------------------
            // 3. Seed Chapters
            // ---------------------
            var chapters = new List<Chapter>
            {
                // One Piece
                new Chapter { Title = "Chapter 1 - Romance Dawn", ContentUrl = "/images/onepiece/ch1.png", Order = 1, ComicId = comics[0].Id },
                new Chapter { Title = "Chapter 2 - Buggy the Clown", ContentUrl = "/images/onepiece/ch2.png", Order = 2, ComicId = comics[0].Id },
                new Chapter { Title = "Chapter 3 - Luffy vs Zoro", ContentUrl = "/images/onepiece/ch3.png", Order = 3, ComicId = comics[0].Id },
                new Chapter { Title = "Chapter 4 - Crew Assemble", ContentUrl = "/images/onepiece/ch4.png", Order = 4, ComicId = comics[0].Id },

                // Naruto
                new Chapter { Title = "Chapter 1 - Uzumaki Naruto", ContentUrl = "/images/naruto/ch1.png", Order = 1, ComicId = comics[1].Id },
                new Chapter { Title = "Chapter 2 - Konohamaru", ContentUrl = "/images/naruto/ch2.png", Order = 2, ComicId = comics[1].Id },
                new Chapter { Title = "Chapter 3 - Chunin Exam", ContentUrl = "/images/naruto/ch3.png", Order = 3, ComicId = comics[1].Id },
                new Chapter { Title = "Chapter 4 - Sasuke vs Naruto", ContentUrl = "/images/naruto/ch4.png", Order = 4, ComicId = comics[1].Id },

                // Bleach
                new Chapter { Title = "Chapter 1 - Death & Soul Society", ContentUrl = "/images/bleach/ch1.png", Order = 1, ComicId = comics[2].Id },
                new Chapter { Title = "Chapter 2 - Bankai", ContentUrl = "/images/bleach/ch2.png", Order = 2, ComicId = comics[2].Id }
            };
            context.Chapters.AddRange(chapters);
            context.SaveChanges();

            // ---------------------
            // 4. Seed Categories
            // ---------------------
            var categories = new List<Category>
            {
                new Category { Name = "Action" },
                new Category { Name = "Adventure" },
                new Category { Name = "Fantasy" },
                new Category { Name = "Comedy" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // ---------------------
            // 5. Liên kết Comic ↔ Category (Many-to-Many)
            // ---------------------
            var comicCategories = new List<ComicCategory>
            {
                new ComicCategory { ComicId = comics[0].Id, CategoryId = categories[0].Id }, // One Piece -> Action
                new ComicCategory { ComicId = comics[0].Id, CategoryId = categories[1].Id }, // One Piece -> Adventure
                new ComicCategory { ComicId = comics[1].Id, CategoryId = categories[0].Id }, // Naruto -> Action
                new ComicCategory { ComicId = comics[1].Id, CategoryId = categories[2].Id }, // Naruto -> Fantasy
                new ComicCategory { ComicId = comics[2].Id, CategoryId = categories[0].Id }, // Bleach -> Action
                new ComicCategory { ComicId = comics[2].Id, CategoryId = categories[2].Id }  // Bleach -> Fantasy
            };
            context.ComicCategories.AddRange(comicCategories);
            context.SaveChanges();
        }
    }
}
