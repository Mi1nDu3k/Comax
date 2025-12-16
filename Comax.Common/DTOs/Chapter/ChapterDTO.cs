using Comax.Common.DTOs.Page;
using System.Collections.Generic;

namespace Comax.Common.DTOs.Chapter
{
    public class ChapterDTO:BaseDto
    {
        public int Id { get; set; }
        public int ComicId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set;}
        public int ChapterNumber { get; set; }

        public DateTime PublishDate { get; set; }
        public ICollection<PageDTO> Pages { get; set; }
    }
}
