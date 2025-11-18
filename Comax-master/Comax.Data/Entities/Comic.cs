namespace Comax.Data.Entities
{
    public class Comic
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public ICollection<ComicCategory> ComicCategories { get; set; }
    }
}