using CommentService.Api.Interfaces.Services;
using CommentService.Api.Data;
using CommentService.Api.Models.Reaction;
using CommentService.Api.Enums;
using Microsoft.EntityFrameworkCore;
using CommentService.Api.Interfaces.Repositories;
using CommentService.Api.Interfaces;

namespace CommentService.Api.Services
{
    public class ReactionService: IReactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task ToggleReaction(Guid UserId, Guid TargetId, ToggleReactionEnum status)
        {
            Reaction? model = await _unitOfWork.Reactions.GetOneAsync(UserId, TargetId);
            
            if (status == ToggleReactionEnum.Undo)
            {
                if (model != null)
                {
                    await _unitOfWork.Reactions.DeleteAsync(model);
                    await _unitOfWork.SaveChangesAsync();

                }
                return;
            }
            bool isLike = status == ToggleReactionEnum.Like;
            if (model == null)
            {
                model = new Reaction
                {
                    UserId = UserId,
                    TargetId = TargetId,
                    IsLike = isLike
                };
                await _unitOfWork.Reactions.AddAsync(model);


            }
            else
            {
                model.IsLike = isLike;
            }


            await _unitOfWork.SaveChangesAsync();
        }
        public Task DeleteReactionsForCommentAsync(Guid videoId)
        {
           return _unitOfWork.Reactions.DeleteReactionsForCommentAsync(videoId);
        }

        
    }

}
