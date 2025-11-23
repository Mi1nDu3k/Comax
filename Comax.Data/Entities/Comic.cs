namespace Comax.Data.Entities
{
    public class Comic
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int AuthorId { get; set; }
        public Author Author { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Chapter> Chapters { get; set; }
        public ICollection<ComicCategory> ComicCategories { get; set; }
    }
}
