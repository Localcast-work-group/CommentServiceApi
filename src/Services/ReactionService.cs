using CommentService.Api.Authorization.Requiremments;
using CommentService.Api.Enums;
using CommentService.Api.Interfaces;
using CommentService.Api.Interfaces.Services;
using CommentService.Api.Models.Comment;
using CommentService.Api.Models.Reaction;
using CommentService.Api.Models.VideoCourse;
using Microsoft.AspNetCore.Authorization;

namespace CommentService.Api.Services
{
    public class ReactionService: IReactionService
    {
        private readonly Serilog.ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IVideoCourseService _videoCourseService;

        public ReactionService(IUnitOfWork unitOfWork, IUserContext userContext, IAuthorizationService authorizationService, IVideoCourseService videoCourseService, Serilog.ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _authorizationService = authorizationService;
            _videoCourseService = videoCourseService;
            _logger = logger;
        }
        public async Task ToggleReaction( Guid TargetId, ToggleReactionEnum status)
        {
            Guid userId = _userContext.UserId.Value;
            Comment? comment = await _unitOfWork.Comments.GetByIdAsync(TargetId);
            VideoCourse? course = await _videoCourseService.GetCourseForVideo(comment.VideoId);
            if (course == null)
            {
                throw new Exception("Video not found in any course");
            }
            
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                _userContext.User,
                course,
                new CanSeeCommentsInCourseRequirement());

            if (!authorizationResult.Succeeded)
            {
                _logger.Warning("User {UserId} attempted to react commment in course {courseId} without privileges.", userId,course.CourseId);
                throw new UnauthorizedAccessException("Access denied.");
            }
            Reaction? model = await _unitOfWork.Reactions.GetOneAsync(userId, TargetId);
            
            if (status == ToggleReactionEnum.Undo)
            {
                if (model != null)
                {
                    _unitOfWork.Reactions.Delete(model);
                    await _unitOfWork.SaveChangesAsync();

                }
                return;
            }
            bool isLike = status == ToggleReactionEnum.Like;
            if (model == null)
            {
                model = new Reaction
                {
                    UserId = userId,
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
        public async Task DeleteReactionsForCommentAsync(Guid commentId)
        {
            Guid userId = _userContext.UserId.Value;
            Comment comment = await _unitOfWork.Comments.GetByIdAsync(commentId);
            VideoCourse? course = await _videoCourseService.GetCourseForVideo(comment.VideoId);
            if (course == null)
            {
                throw new Exception("Video not found in any course");
            }
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                _userContext.User,
                course.CourseId,
                new CanModerateCommentInCourseRequirement());

            if (!authorizationResult.Succeeded && _userContext.UserId != comment.UserId)
            {
                _logger.Warning("User {UserId} attempted to delete reactions in course {courseId} without privileges.", userId, course.CourseId);
                throw new UnauthorizedAccessException("Access denied.");
            }
           _unitOfWork.Reactions.DeleteReactionsForComment(commentId);
           await _unitOfWork.SaveChangesAsync();
        }

        
    }

}
