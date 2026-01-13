

namespace Comax.Common.DTOs.Comment
{
    public class CommentCreateDTO
    {
        public string Content { get; set; } = null!;
        public int ComicId { get; set; }
        public int UserId { get; set; }
        public int? ParentId { get; set; }
    }
}
