using System.Collections.Generic;

namespace Comax.Common.DTOs.Comic
{
    public class ComicCreateDTO : BaseDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; }

        public int AuthorId { get; set; }
        public string CategoryID { get; set; }


    }
}
