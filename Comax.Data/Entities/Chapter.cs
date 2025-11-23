namespace Comax.Data.Entities
{
    public class Chapter
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public string Content { get; set; }
        public int ComicId { get; set; }
        public Comic Comic { get; set; }
    }
}
