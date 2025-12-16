using Microsoft.AspNetCore.Http;

namespace Comax.Shared.DTOs
{
    public class ChapterCreateWithImagesDTO
    {
        public int ComicId { get; set; }
        public double ChapterNumber { get; set; }
        public string? Title { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}