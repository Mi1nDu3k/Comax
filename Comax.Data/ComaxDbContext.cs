using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Comax.Data.Entities;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Comax.Data
{
    public class ComaxDbContext : DbContext
    {
        public ComaxDbContext(DbContextOptions<ComaxDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comic> Comics { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Rating> Ratings { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<ComicCategory> ComicCategories { get; set; }

        // --- CẤU HÌNH ---
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Bỏ qua cảnh báo Query Filter
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Các cấu hình Relation
            // taoj configuration rieng cho tung entity
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<Comic>().HasOne(c => c.Author).WithMany(a => a.Comics).HasForeignKey(c => c.AuthorId);

            modelBuilder.Entity<Chapter>().HasOne(ch => ch.Comic).WithMany(c => c.Chapters).HasForeignKey(ch => ch.ComicId);

            modelBuilder.Entity<ComicCategory>().HasKey(cc => new { cc.ComicId, cc.CategoryId });
            modelBuilder.Entity<ComicCategory>().HasOne(cc => cc.Comic).WithMany(c => c.ComicCategories).HasForeignKey(cc => cc.ComicId);
            modelBuilder.Entity<ComicCategory>().HasOne(cc => cc.Category).WithMany(c => c.ComicCategories).HasForeignKey(cc => cc.CategoryId);

            modelBuilder.Entity<Rating>().HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);
            modelBuilder.Entity<Rating>().HasOne(r => r.Comic).WithMany().HasForeignKey(r => r.ComicId);

            modelBuilder.Entity<Comment>().HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
            modelBuilder.Entity<Comment>().HasOne(c => c.Comic).WithMany().HasForeignKey(c => c.ComicId);

            // Cấu hình Soft Delete tự động
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

        private static void SetSoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : BaseEntity
        {
            modelBuilder.Entity<T>().HasQueryFilter(x => !x.IsDeleted);
        }

        // --- INTERCEPTOR: Tự động gán Slug & CreatedAt ---
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

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                // 1. Tự động gán CreatedAt/UpdatedAt
                if (entry.Entity is BaseEntity baseEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        baseEntity.CreatedAt = DateTime.UtcNow;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        baseEntity.UpdatedAt = DateTime.UtcNow;
                    }
                }

                // 2. TỰ ĐỘNG SINH SLUG (COMIC, CATEGORY, CHAPTER)
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    // Xử lý Comic
                    if (entry.Entity is Comic comic)
                    {
                        if (string.IsNullOrEmpty(comic.Slug) && !string.IsNullOrEmpty(comic.Title))
                        {
                            comic.Slug = GenerateSlug(comic.Title);
                        }
                        if (string.IsNullOrEmpty(comic.Slug)) comic.Slug = $"comic-{Guid.NewGuid()}";
                    }
                    // Xử lý Category (Thêm mới)
                    else if (entry.Entity is Category category)
                    {
                        if (string.IsNullOrEmpty(category.Slug) && !string.IsNullOrEmpty(category.Name))
                        {
                            category.Slug = GenerateSlug(category.Name);
                        }
                        if (string.IsNullOrEmpty(category.Slug)) category.Slug = $"cat-{Guid.NewGuid()}";
                    }
                    // Xử lý Chapter (Thêm mới)
                    else if (entry.Entity is Chapter chapter)
                    {
                        if (string.IsNullOrEmpty(chapter.Slug) && !string.IsNullOrEmpty(chapter.Title))
                        {
                            chapter.Slug = GenerateSlug(chapter.Title);
                        }
                        if (string.IsNullOrEmpty(chapter.Slug)) chapter.Slug = $"chap-{Guid.NewGuid()}";
                    }
                }
            }
        }

        private string GenerateSlug(string phrase)
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