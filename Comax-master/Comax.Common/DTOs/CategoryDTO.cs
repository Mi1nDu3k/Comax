namespace Comax.Common.DTOs
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class CategoryCreateDTO
    {
        public string Name { get; set; } = "";
    }

    public class CategoryUpdateDTO
    {
        public string Name { get; set; } = "";
    }
}
