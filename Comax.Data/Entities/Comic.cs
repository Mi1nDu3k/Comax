namespace Comax.Data.Entities
{
    // Thêm kế thừa BaseEntity
    public class Comic : BaseEntity
    {
        // Xóa public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int AuthorId { get; set; }
        public Author Author { get; set; }

        // Xóa CreatedAt nếu bạn muốn dùng CreatedAt chung của BaseEntity
        // Nếu muốn giữ lại logic riêng thì cẩn thận trùng tên. 
        // BaseEntity đã có: public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Nên ta xóa dòng dưới đây đi:
        // public DateTime CreatedAt { get; set; } 

        public ICollection<Chapter> Chapters { get; set; }
        public ICollection<ComicCategory> ComicCategories { get; set; }
    }
}