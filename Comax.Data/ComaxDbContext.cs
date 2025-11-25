using Microsoft.EntityFrameworkCore;
using Comax.Data.Entities;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Indexes & Relations
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<Comic>()
                .HasOne(c => c.Author)
                .WithMany(a => a.Comics)
                .HasForeignKey(c => c.AuthorId);

            modelBuilder.Entity<Chapter>()
                .HasOne(ch => ch.Comic)
                .WithMany(c => c.Chapters)
                .HasForeignKey(ch => ch.ComicId);

            modelBuilder.Entity<ComicCategory>()
                .HasKey(cc => new { cc.ComicId, cc.CategoryId });

            modelBuilder.Entity<ComicCategory>()
                .HasOne(cc => cc.Comic)
                .WithMany(c => c.ComicCategories)
                .HasForeignKey(cc => cc.ComicId);

            modelBuilder.Entity<ComicCategory>()
                .HasOne(cc => cc.Category)
                .WithMany(c => c.ComicCategories)
                .HasForeignKey(cc => cc.CategoryId);

            modelBuilder.Entity<Rating>()
               .HasOne(r => r.User)
               .WithMany()
               .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Comic)
                .WithMany()
                .HasForeignKey(r => r.ComicId);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Comic)
                .WithMany()
                .HasForeignKey(c => c.ComicId);
        }
    }
}
