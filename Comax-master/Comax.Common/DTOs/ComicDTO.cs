using System.Collections.Generic;


namespace Comax.Common.DTOs
{
    public class ComicDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public AuthorDTO Author { get; set; } = null!;
        public ICollection<ChapterDTO> Chapters { get; set; } = new List<ChapterDTO>();
        public ICollection<CategoryDTO> Categories { get; set; } = new List<CategoryDTO>();
    }
}