namespace Comax.Data.Entities
{
    public class ComicCategory
    {
        public int ComicId { get; set; }
        public Comic Comic { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public ICollection<ComicCategory> ComicCategories { get; set; } = new List<ComicCategory>();
    }
}