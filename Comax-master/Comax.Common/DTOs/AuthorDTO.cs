namespace Comax.Common.DTOs
{
    public class AuthorDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class AuthorCreateDTO
    {
        public string Name { get; set; } = "";
    }

    public class AuthorUpdateDTO
    {
        public string Name { get; set; } = "";
    }
}
