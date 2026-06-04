using CommentService.Api.Models.Reaction;

namespace CommentService.Api.Interfaces.Repositories
{
    public interface IReactionRepository
    {
        public void DeleteReactionsForComment(Guid commentId);
        public Task<Reaction?> GetOneAsync(Guid commentId,Guid userId);
        public void Delete(Reaction model);
        public Task AddAsync(Reaction model);
    }
}
