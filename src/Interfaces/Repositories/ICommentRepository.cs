using CommentService.Api.Models.Comment;

namespace CommentService.Api.Interfaces.Repositories
{
    public interface ICommentRepository
    {
        public Task AddAsync(Comment comment);
        public Task<IQueryable<Comment>> GetAllForVideo(Guid VideoId, bool includeReactions);
        public Task DeleteAsync(Comment comment);
        public Task<Comment> GetByIdAsync(Guid id);
        public Task DeleteChildren(Guid parentId);

    }
}
