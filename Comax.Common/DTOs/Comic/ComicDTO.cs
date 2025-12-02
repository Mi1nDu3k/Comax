using System.Collections.Generic;

namespace Comax.Common.DTOs.Comic
{
    public class ComicDTO:BaseDto
    {
        public string Title { get; set; } = null!;
        public string CoverImage { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Description { get; set; } = null!;

        public string Status { get; set; } = null!;
    }
}
