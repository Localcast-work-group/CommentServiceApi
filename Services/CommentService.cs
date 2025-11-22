using CommentService.Api.Data;
using CommentService.Api.Interfaces;
using CommentService.Api.Interfaces.Repositories;
using CommentService.Api.Interfaces.Services;
using CommentService.Api.Models.Comment;
using CommentService.Api.Models.Comment.DTOs;
using CommentService.Api.Models.Reaction;
using Microsoft.EntityFrameworkCore;

namespace CommentService.Api.Services
{
    public class CommentService : ICommentService
    {
        private readonly IReactionService _reactionService;
        private readonly IUnitOfWork _unitOfWork;

        public CommentService(IUnitOfWork unitOfWork, IReactionService reactionService)
        {
            _unitOfWork = unitOfWork;
            _reactionService = reactionService;
        }
        public async Task<GetCommentDTO> Add(CreateCommentDTO createCommentDTO, Guid userId,string userName)
        {
            Comment model = new Comment
            {
                Content = createCommentDTO.Content,
                CreatedAt = DateTime.UtcNow,
                ParentCommentId = string.IsNullOrEmpty( createCommentDTO.ParentId ) ? null : Guid.Parse(createCommentDTO.ParentId),
                UserId = userId,
                VideoId = createCommentDTO.VideoId,
                UserName = userName
            };
            await _unitOfWork.Comments.AddAsync(model);
            await _unitOfWork.SaveChangesAsync();
            
            return new GetCommentDTO
            {
                Id = model.Id,
                ParentId = model.ParentCommentId,
                UserName = model.UserName,
                TotalLikes = 0,
                TotalDisLikes = 0,
                Content = model.Content,
                CreatedAt = model.CreatedAt,
                IsLiked = null,
                IsDisLiked = null
            };
        }
        //userId null - skip permission check
        public async Task Delete(Guid id,Guid? userId = null)
        {
            Comment? model =  await _unitOfWork.Comments.GetByIdAsync(id);
            if (model != null)
            {
         
                if (userId.HasValue &&  model.UserId != userId)
                {
                    throw new UnauthorizedAccessException();
                }

                await _reactionService.DeleteReactionsForCommentAsync(id);
                await _unitOfWork.Comments.DeleteChildren(id);
                await _unitOfWork.Comments.DeleteAsync(model);
                await _unitOfWork.SaveChangesAsync();
            }

        }

        
        public async Task<List<GetCommentDTO>> GetAllForVideo(Guid VideoId, Guid? UserId)
        {
            IQueryable<Comment> comments = await _unitOfWork.Comments.GetAllForVideo(VideoId,true);
            IQueryable<GetCommentDTO> model = comments.Select(x => new GetCommentDTO
            {
                Id = x.Id,
                UserId = x.UserId,
                ParentId = x.ParentCommentId,
                UserName = x.UserName,
                TotalLikes = x.Reactions.Where(r => r.IsLike).Count(),
                TotalDisLikes = x.Reactions.Where(r => !r.IsLike).Count(),
                Content = x.Content,
                CreatedAt = x.CreatedAt,
                IsLiked = UserId == null ? null : x.Reactions.Where(r => r.IsLike && r.UserId == UserId).Count() > 0,
                IsDisLiked = UserId == null ? null : x.Reactions.Where(r => !r.IsLike && r.UserId == UserId).Count() > 0
            });

            return await model.ToListAsync();
        }

    }
}
