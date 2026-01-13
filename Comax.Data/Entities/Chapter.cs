using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Comax.Data.Entities
{
    // Thêm kế thừa BaseEntity
    [Index(nameof(ComicId), nameof(Slug), IsUnique = true)]
    public class Chapter : BaseEntity
    {
        // Xóa public int Id { get; set; }
        public string Title { get; set; }
        [Required]
        [MaxLength(100)]
        public string Slug { get; set; }  
        public int ComicId { get; set; }
        public int Order { get; set; }
        public int ChapterNumber { get; set; }
        public string? Content { get; set; }
        public Comic Comic { get; set; }
        public DateTime PublishDate { get; set; } = DateTime.UtcNow;
        public virtual ICollection<Page> Pages { get; set; } = new List<Page>();
    }
}