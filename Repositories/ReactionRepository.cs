using CommentService.Api.Data;
using CommentService.Api.Interfaces.Repositories;
using CommentService.Api.Models.Reaction;
using Microsoft.EntityFrameworkCore;

namespace CommentService.Api.Repositories
{
    public class ReactionRepository : IReactionRepository
    {
        private readonly ApplicationDbContext _dBContext;


        public ReactionRepository(ApplicationDbContext dBContext)
        {
            _dBContext = dBContext;
        }

        public async Task AddAsync(Reaction model)
        {
            await _dBContext.AddAsync(model);
        }

        public Task DeleteAsync(Reaction model)
        {
            _dBContext.Remove(model);
            return Task.CompletedTask;
        }


        public Task DeleteReactionsForCommentAsync(Guid videoId)
        {
             _dBContext.Reactions.RemoveRange(_dBContext.Reactions.Where(r => r.TargetId == videoId));
              return Task.CompletedTask;
        }

        public Task<Reaction> GetOneAsync(Guid videoId, Guid userId)
        {
            return _dBContext.Reactions.Where(x => x.TargetId == videoId && x.UserId == userId).FirstOrDefaultAsync();
        }
    }
}
