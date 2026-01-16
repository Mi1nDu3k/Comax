using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Comax.Data.Entities
{
    [ Index( nameof( Slug ), IsUnique = true )]
    public class Comic : BaseEntity
    {
       
        public string Title { get; set; }
        [Required]
        [MaxLength(255)]
        public string Slug { get; set; }
        public string? Description { get; set; }

        public string CoverImage { get; set; } // Link ảnh bìa
        public float Rating { get; set; }      // Điểm đánh giá (VD: 4.5)
        public string Status { get; set; }
        public int ViewCount { get; set; }     // Số lượt xem
        public int AuthorId { get; set; }
        public Author Author { get; set; }
     

  

        public ICollection<Chapter> Chapters { get; set; }
        public ICollection<ComicCategory> ComicCategories { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}