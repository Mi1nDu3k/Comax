namespace Comax.Common.DTOs
{
    public class ComicCreateDTO
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int AuthorId { get; set; }
        public List<int>? CategoryIds { get; set; }= new List<int>();
    }
}