using CommentService.Api.Enums;

namespace CommentService.Api.Interfaces.Services
{
    public interface IReactionService
    {
        public Task ToggleReaction( Guid TargetId, ToggleReactionEnum status);
        public Task DeleteReactionsForCommentAsync(Guid CommentId);

    }
}
