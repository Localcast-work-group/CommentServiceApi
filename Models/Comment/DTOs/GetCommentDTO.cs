namespace CommentService.Api.Models.Comment.DTOs
{
    public class GetCommentDTO
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; }
        public int TotalLikes { get; set; }
        public int TotalDisLikes { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool? IsLiked { get; set; }
        public bool? IsDisLiked { get; set; }
    }
}
