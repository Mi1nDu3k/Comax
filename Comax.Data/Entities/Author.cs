using Comax.Common.DTOs;

namespace Comax.Data.Entities
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Comic> Comics { get; set; }
    }
}
