using System;

namespace Comax.Data.Entities
{
    public class Favorite
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int ComicId { get; set; }
        public Comic Comic { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}