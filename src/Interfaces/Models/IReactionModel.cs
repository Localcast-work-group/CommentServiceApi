namespace CommentService.Api.Interfaces.Models
{
    public interface IReactionModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TargetId { get; set; }
        public bool IsLike { get; set; }
    }
}
