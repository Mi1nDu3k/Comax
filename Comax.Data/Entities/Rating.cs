namespace Comax.Data.Entities
{
    public class Rating : BaseEntity 
    {
        public int ComicId { get; set; }
        public int UserId { get; set; }
        public int Score { get; set; }

        public string? Comment { get; set; }

   
        public virtual Comic Comic { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
Z