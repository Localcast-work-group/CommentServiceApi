using CommentService.Api.Data;
using CommentService.Api.Interfaces.Repositories;
using CommentService.Api.Models.Comment;
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

        public void Delete(Comment comment)
        {
            _dBContext.Set<Comment>().Remove(comment);
        }

        public  void DeleteChildren(Guid parentId)
        {
             _dBContext.Set<Comment>().RemoveRange(_dBContext.Set<Comment>().Where(x => x.ParentCommentId == parentId));
        }

        public  IQueryable<Comment> GetAllForVideo(Guid VideoId,bool includeReactions)
        {
            IQueryable<Comment> query = _dBContext.Comments;
            if (includeReactions)
            {
                query = query.Include(x => x.Reactions);
            }
            return query.Where(x => x.VideoId == VideoId);

        }

        public Task<Comment?> GetByIdAsync(Guid id)
        {
            return _dBContext.Set<Comment>().FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
