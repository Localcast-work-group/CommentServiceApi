using CommentService.Api.Interfaces.Models;

namespace CommentService.Api.Models.Reaction
{
    public class Reaction : IReactionModel
    {
        public Guid Id { get; set; }
        public Guid TargetId { get; set; }
        public Guid UserId { get; set; }
        public bool IsLike { get; set; } // true = like, false = dislike
        public virtual Comment.Comment Comment { get; set; }

    }
}
