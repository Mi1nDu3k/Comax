using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comax.Data.Entities
{
    [Table("Pages")]
    public class Page : BaseEntity 
    {
        [Required]
        public string ImageUrl { get; set; }

        public int Index { get; set; }

        public string? FileName { get; set; }

        // --- Foreign Key ---
        [Required]
        public int ChapterId { get; set; }

        [ForeignKey("ChapterId")]
        public virtual Chapter Chapter { get; set; }
    }
}