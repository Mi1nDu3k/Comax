namespace Comax.Data.Entities
{
    // Thêm kế thừa BaseEntity
    public class Author : BaseEntity
    {
        // Xóa public int Id { get; set; } vì đã có ở BaseEntity
        public string Name { get; set; }
        public ICollection<Comic> Comics { get; set; }
    }
}