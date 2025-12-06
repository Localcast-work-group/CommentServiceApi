using CommentService.Api.Interfaces.Repositories;

namespace CommentService.Api.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICommentRepository Comments { get; }
        IReactionRepository Reactions { get; }
        IVideoCourseRepository VideoCourses { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
