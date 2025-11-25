

namespace Comax.Data.Entities
{
    public class Rating : BaseEntity
    {
        public int Score { get; set; } // 1-5
        public string? Comment { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int ComicId { get; set; }
        public Comic? Comic { get; set; }
    }
}
