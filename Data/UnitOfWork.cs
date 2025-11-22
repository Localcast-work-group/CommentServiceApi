using CommentService.Api.Interfaces;
using CommentService.Api.Interfaces.Repositories;
using CommentService.Api.Repositories;

namespace CommentService.Api.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        private ICommentRepository _commentRepository;
        private IReactionRepository _reactionRepository;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ICommentRepository Comments
        {
            get
            {
                return _commentRepository ??= new CommentRepository(_dbContext);
            }
        }

        public IReactionRepository Reactions
        {
            get
            {
                return _reactionRepository ??= new ReactionRepository(_dbContext);
            }
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
