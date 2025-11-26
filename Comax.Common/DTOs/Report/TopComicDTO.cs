namespace Comax.Common.DTOs.Report
{
    public class TopComicDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ViewCount { get; set; }
        public double AverageRating { get; set; } 
        public int RatingCount { get; set; }      
    }
}