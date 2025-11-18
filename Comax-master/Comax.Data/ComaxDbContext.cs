using Comax.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Comax.Data
{
    public class ComaxDbContext : DbContext
    {
        public ComaxDbContext(DbContextOptions<ComaxDbContext> options)
            : base(options) { }

        public DbSet<Comic> Comics { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ComicCategory> ComicCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ComicCategory>()
                .HasKey(x => new { x.ComicId, x.CategoryId });
        }
    }

}