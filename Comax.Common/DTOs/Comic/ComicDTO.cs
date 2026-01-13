using System.Collections.Generic;

namespace Comax.Common.DTOs.Comic
{
    public class ComicDTO : BaseDto
    {
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!;
        
        public int ViewCount { get; set; }
        public string ThumbnailUrl { get; set; }

        public int AuthorId { get; set; }
        public string AuthorName { get; set; }

        public List<int> CategoryIds { get; set; }
        public int? LatestChapterNumber { get; set; }
        public DateTime? LatestChapterDate { get; set; }
        public double Rating { get; set; }
        public DateTime CreatedAt { get; set; } 
        public List<string> CategoryNames { get; set; } = new();

    }
}