using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Comax.Data.Entities
{
    [Index(nameof(Slug), IsUnique = true)]
    public class Category : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Slug { get; set; }
        public string Name { get; set; }
        public ICollection<ComicCategory> ComicCategories { get; set; }
    }
}