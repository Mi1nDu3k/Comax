using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Comax.Common.DTOs.Chapter
{
    public class ChapterCreateWithImagesDTO
    {
        public int ComicId { get; set; }
        public string Title { get; set; }
        public int ChapterNumber { get; set; }

        public List<IFormFile> Images { get; set; }
    }
}