using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Comax.Data.Entities
{
    public class ComicCategory
    {
        public int ComicId { get; set; }
        public Comic Comic { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
