namespace Comax.Common.DTOs.Report
{
    public class TopComicDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ViewCount { get; set; }

        public string ThumbnailUrl { get; set; }
        public float Rating { get; set; }

        public int Status { get; set; }
    }
}