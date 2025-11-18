namespace Comax.Data.Entities
{
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<Comic> Comics { get; set; } = new List<Comic>();
    }
}