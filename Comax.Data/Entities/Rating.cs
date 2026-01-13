using System.ComponentModel.DataAnnotations.Schema;
namespace Comax.Data.Entities
{
    public class Rating : BaseEntity 
    {

        public int Score { get; set; }
        public string? Comment { get; set; }

        public int ComicId { get; set; }

        [ForeignKey("ComicId")] 
        public virtual Comic Comic { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")] 
        public virtual User User { get; set; }
    }
}