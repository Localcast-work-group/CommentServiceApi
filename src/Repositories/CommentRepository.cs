using CommentService.Api.Data;
using CommentService.Api.Interfaces.Repositories;
using CommentService.Api.Interfaces.Services;
using CommentService.Api.Models.Comment;
using CommentService.Api.Models.Comment.DTOs;
using CommentService.Api.Models.Reaction;
using Microsoft.EntityFrameworkCore;

namespace CommentService.Api.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _dBContext;


        public CommentRepository(ApplicationDbContext dBContext)
        {
            _dBContext = dBContext;
        }
        

        public async Task AddAsync(Comment comment)
        {
             await _dBContext.AddAsync(comment);
        }

        public Task DeleteAsync(Comment comment)
        {
            _dBContext.Set<Comment>().Remove(comment);
            return Task.CompletedTask;
        }

        public  Task DeleteChildren(Guid parentId)
        {
             _dBContext.Set<Comment>().RemoveRange(_dBContext.Set<Comment>().Where(x => x.ParentCommentId == parentId));
               return Task.CompletedTask;
        }

        public  Task<IQueryable<Comment>> GetAllForVideo(Guid VideoId,bool includeReactions)
        {
            IQueryable<Comment> query = _dBContext.Comments;
            if (includeReactions)
            {
                query = query.Include(x => x.Reactions);
            }
            return Task.FromResult(query.Where(x => x.VideoId == VideoId));

        }

        public Task<Comment> GetByIdAsync(Guid id)
        {
            return _dBContext.Set<Comment>().FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
