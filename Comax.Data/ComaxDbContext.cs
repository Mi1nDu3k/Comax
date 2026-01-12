using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Comax.Data.Entities;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Globalization;

namespace Comax.Data
{
    public class ComaxDbContext : DbContext
    {
        public ComaxDbContext(DbContextOptions<ComaxDbContext> options) : base(options) { }

        // --- DB SETS ---
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comic> Comics { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Rating> Ratings { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<ComicCategory> ComicCategories { get; set; }
        public DbSet<Favorite> Favorites { get; set; } = null!;
        public DbSet<Page> Pages { get; set; }
        public DbSet<History> Histories { get; set; }

        // --- CẤU HÌNH ---
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Bỏ qua cảnh báo Query Filter khi Include (Required Navigation)
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình Relation & Index
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<Comic>().HasOne(c => c.Author).WithMany(a => a.Comics).HasForeignKey(c => c.AuthorId);

            modelBuilder.Entity<Chapter>().HasOne(ch => ch.Comic).WithMany(c => c.Chapters).HasForeignKey(ch => ch.ComicId);

            // Composite Key cho bảng trung gian
            modelBuilder.Entity<ComicCategory>().HasKey(cc => new { cc.ComicId, cc.CategoryId });
            modelBuilder.Entity<ComicCategory>().HasOne(cc => cc.Comic).WithMany(c => c.ComicCategories).HasForeignKey(cc => cc.ComicId);
            modelBuilder.Entity<ComicCategory>().HasOne(cc => cc.Category).WithMany(c => c.ComicCategories).HasForeignKey(cc => cc.CategoryId);

            // Cấu hình Rating
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Comic)
                .WithMany(c => c.Ratings)
                .HasForeignKey(r => r.ComicId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany() // Nếu User có List<Rating> thì sửa thành u => u.Ratings
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- CẤU HÌNH COMMENT (QUAN TRỌNG ĐÃ SỬA) ---
            modelBuilder.Entity<Comment>(entity =>
            {
                // 1. Quan hệ với User
                entity.HasOne(c => c.User)
                      .WithMany()
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // 2. Quan hệ với Comic
                entity.HasOne(c => c.Comic)
                      .WithMany() // Nếu Comic có ICollection<Comment> thì sửa thành c.Comments
                      .HasForeignKey(c => c.ComicId)
                      .OnDelete(DeleteBehavior.Cascade);

                // 3. QUAN TRỌNG NHẤT: Quan hệ Cha - Con (Replies)
                // Giúp .Include(c => c.Replies) hoạt động chính xác
                entity.HasOne(c => c.ParentComment)
                      .WithMany(c => c.Replies)
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa cha thì xóa luôn con
            });
            // ---------------------------------------------

            modelBuilder.Entity<Favorite>().HasKey(f => new { f.UserId, f.ComicId });
            modelBuilder.Entity<Favorite>().HasOne(f => f.User).WithMany().HasForeignKey(f => f.UserId);
            modelBuilder.Entity<Favorite>().HasOne(f => f.Comic).WithMany().HasForeignKey(f => f.ComicId);

            modelBuilder.Entity<Notification>().HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId);

            // 2. Cấu hình Soft Delete tự động bằng Reflection
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(ComaxDbContext)
                        .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)
                        ?.MakeGenericMethod(entityType.ClrType);
                    method?.Invoke(null, new object[] { modelBuilder });
                }
            }
        }

        // Hàm helper cho Soft Delete
        private static void SetSoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
        {
            modelBuilder.Entity<T>().HasQueryFilter(x => !x.IsDeleted);
        }

        // --- SAVE CHANGES INTERCEPTOR ---
        public override int SaveChanges()
        {
            OnBeforeSaving();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(cancellationToken);
        }

        // --- LOGIC XỬ LÝ TRƯỚC KHI LƯU (IN-MEMORY ONLY) ---
        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                // 1. Tự động gán CreatedAt/UpdatedAt
                if (entry.Entity is BaseEntity baseEntity)
                {
                    var now = DateTime.UtcNow;
                    if (entry.State == EntityState.Added)
                    {
                        baseEntity.CreatedAt = now;
                        if (baseEntity.UpdatedAt == default) baseEntity.UpdatedAt = now;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        baseEntity.UpdatedAt = now;
                    }
                }

                // 2. Tự động sinh Slug (Nếu chưa có)
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    if (entry.Entity is Comic comic)
                    {
                        if (string.IsNullOrEmpty(comic.Slug) && !string.IsNullOrEmpty(comic.Title))
                            comic.Slug = GenerateSlug(comic.Title);
                    }
                    else if (entry.Entity is Category category)
                    {
                        if (string.IsNullOrEmpty(category.Slug) && !string.IsNullOrEmpty(category.Name))
                            category.Slug = GenerateSlug(category.Name);
                    }
                    else if (entry.Entity is Chapter chapter)
                    {
                        if (string.IsNullOrEmpty(chapter.Slug) && !string.IsNullOrEmpty(chapter.Title))
                            chapter.Slug = GenerateSlug(chapter.Title);
                    }
                }
            }
        }

        // --- HÀM TẠO SLUG (Hỗ trợ Tiếng Việt) ---
        private string GenerateSlug(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return "";
            string str = phrase.ToLowerInvariant();

            str = RemoveDiacritics(str);
            str = Regex.Replace(str, @"đ", "d");

            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = Regex.Replace(str, @"\s", "-");

            return str;
        }


        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}