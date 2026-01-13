namespace Comax.Common.DTOs
{
    public class RatingCreateDTO
    {
        public int Score { get; set; }
        public string? Comment { get; set; }
        public int ComicId { get; set; }
        public int UserId { get; set; }
    }
}