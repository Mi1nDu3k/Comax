namespace Comax.Common.DTOs.History
{
    public class HistoryDTO
    {
        public int Id { get; set; } 
        public int ComicId { get; set; }
        public string ComicTitle { get; set; }
        public string ComicImage { get; set; }

        public int ChapterId { get; set; }
        public string ChapterNumber { get; set; }

        public DateTime LastReadTime { get; set; }
    }

    
}