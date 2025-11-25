namespace Comax.Data.Entities
{
    // Thêm kế thừa BaseEntity
    public class Chapter : BaseEntity
    {
        // Xóa public int Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public string Content { get; set; }
        public int ComicId { get; set; }
        public Comic Comic { get; set; }
    }
}