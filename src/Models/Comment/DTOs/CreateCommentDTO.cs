namespace CommentService.Api.Models.Comment.DTOs
{
    public class CreateCommentDTO
    {
        public Guid VideoId { get; set; }
        public string ParentId { get; set; }
        public string Content { get; set; }
    }
}
