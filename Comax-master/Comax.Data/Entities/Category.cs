namespace Comax.Data.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<ComicCategory> ComicCategories { get; set; } = new List<ComicCategory>();
    }
}