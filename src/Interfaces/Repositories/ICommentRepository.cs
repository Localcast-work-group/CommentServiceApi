using CommentService.Api.Models.Comment;

namespace CommentService.Api.Interfaces.Repositories
{
    public interface ICommentRepository
    {
        public Task AddAsync(Comment comment);
        public IQueryable<Comment> GetAllForVideo(Guid VideoId, bool includeReactions);
        public void Delete(Comment comment);
        public Task<Comment?> GetByIdAsync(Guid id);
        public void DeleteChildren(Guid parentId);

    }
}
