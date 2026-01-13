using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Comax.Data.Entities
{
    [Table("Histories")] 
    public class History : BaseEntity 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int ComicId { get; set; }
        [ForeignKey("ComicId")]
        public virtual Comic Comic { get; set; }

        public int ChapterId { get; set; }
        [ForeignKey("ChapterId")]
        public virtual Chapter Chapter { get; set; }

        public DateTime LastReadTime { get; set; } = DateTime.UtcNow;
    }
}