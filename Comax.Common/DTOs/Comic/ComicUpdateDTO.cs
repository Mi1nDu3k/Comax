using System.Collections.Generic;

namespace Comax.Common.DTOs.Comic
{
    public class ComicUpdateDTO: BaseDto
    {
        public string? Title { get; set; }
        public int? number { get; set; }
        public string? Descriptiom { get; set; }
    }
}
