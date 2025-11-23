namespace Comax.Common.DTOs.Chapter
{
    public class ChapterUpdateDTO:BaseDto
    {
        public string? Title { get; set; }
        public int? number { get; set; }
        public int? Order { get; set; }
        public string? Content { get; set; }
    }
}
