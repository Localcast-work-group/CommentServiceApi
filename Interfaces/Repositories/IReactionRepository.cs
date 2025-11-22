using CommentService.Api.Models.Reaction;

namespace CommentService.Api.Interfaces.Repositories
{
    public interface IReactionRepository
    {
        public Task DeleteReactionsForCommentAsync(Guid commentId);
        public Task<Reaction> GetOneAsync(Guid commentId,Guid userId);
        public Task DeleteAsync(Reaction model);
        public Task AddAsync(Reaction model);
    }
}
