namespace Comax.Common.DTOs
{
    public class ChapterUpdateDTO
    {
        public string Title { get; set; } = null!;
        public string ContentUrl { get; set; } = null!;
        public int Order { get; set; }
    }
}