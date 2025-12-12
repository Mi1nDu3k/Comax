namespace Comax.Common.DTOs.Chapter
{
    public class ChapterCreateDTO:BaseDto
    {
        public int ComicId { get; set; }
        public string Title { get; set; } = null!;
        public int ChapterNumber { get; set; }

        public string Content { get; set; }
    }
}
