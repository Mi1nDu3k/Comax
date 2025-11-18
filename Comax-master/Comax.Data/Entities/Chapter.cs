namespace Comax.Data.Entities
{
    public class Chapter
    {
        public int Id { get; set; }
        public int ComicId { get; set; }
        public Comic Comic { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string ContentUrl { get; set; } = null!;
        public int Order { get; set; }
    }
}