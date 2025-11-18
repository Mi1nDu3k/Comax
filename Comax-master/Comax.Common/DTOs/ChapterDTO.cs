namespace Comax.Common.DTOs
{
    public class ChapterDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string ContentUrl { get; set; } = null!;
        public int Order { get; set; }
        public int ComicId { get; set; }
    }
}