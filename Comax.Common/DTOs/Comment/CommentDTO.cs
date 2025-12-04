namespace Comax.Common.DTOs.Comment
{
    public class CommentDTO : BaseDto
    {
        public string Content { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } 
        public int ComicId { get; set; }
        public int? ParentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}