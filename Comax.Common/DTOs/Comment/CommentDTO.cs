namespace Comax.Common.DTOs.Comment
{
    public class CommentDTO : BaseDto
    {
        public int ComicId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }     
        public string UserAvatar { get; set; }   
        public string Content { get; set; }
        public int? ParentId { get; set; }
        public int ReplyCount { get; set; }
        public List<CommentDTO> Replies { get; set; } = new List<CommentDTO>();
    }
}