namespace Comax.Data.Entities
{
    public class Comment : BaseEntity
    {
        public string Content { get; set; } = null!;

        public int UserId { get; set; }
        public User? User { get; set; }

        public int ComicId { get; set; }
        public Comic? Comic { get; set; }
    }
}
