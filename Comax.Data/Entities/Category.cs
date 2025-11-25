namespace Comax.Data.Entities
{
    // Thêm kế thừa BaseEntity
    public class Category : BaseEntity
    {
        // Xóa public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ComicCategory> ComicCategories { get; set; }
    }
}