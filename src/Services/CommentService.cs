using CommentService.Api.Authorization.Requiremments;
using CommentService.Api.Interfaces;
using CommentService.Api.Interfaces.Services;
using CommentService.Api.Models.Comment;
using CommentService.Api.Models.Comment.DTOs;
using CommentService.Api.Models.VideoCourse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CommentService.Api.Services
{
    public class CommentService : ICommentService
    {
        private readonly IReactionService _reactionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly Serilog.ILogger _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IVideoCourseService _videoCourseService;

        public CommentService(IUnitOfWork unitOfWork, IReactionService reactionService, IAuthorizationService authorizationService, IUserContext userContext, Serilog.ILogger logger, IVideoCourseService videoCourseService)
        {
            _unitOfWork = unitOfWork;
            _reactionService = reactionService;
            _logger = logger;
            _authorizationService = authorizationService;
            _userContext = userContext;
            _videoCourseService = videoCourseService;
        }
        [Authorize]
        public async Task<GetCommentDTO> Add(CreateCommentDTO createCommentDTO)
        {
            var authResult = _authorizationService.AuthorizeAsync(
                _userContext.User,
                createCommentDTO.CourseId,
                new CanAddCommentInCourseRequirement()
                );
            if (!authResult.Result.Succeeded) throw new UnauthorizedAccessException();
            Comment model = new Comment
            {
                Content = createCommentDTO.Content,
                CreatedAt = DateTime.UtcNow,
                ParentCommentId = string.IsNullOrEmpty( createCommentDTO.ParentId ) ? null : Guid.Parse(createCommentDTO.ParentId),
                UserId = _userContext.UserId.Value,
                VideoId = createCommentDTO.VideoId,
                UserName = _userContext.User.Identity.Name ?? ""
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
        [Authorize]

        public async Task Delete(Guid id)
        {
            Guid? userId = _userContext.UserId;

            Comment? model =  await _unitOfWork.Comments.GetByIdAsync(id);
            if (model != null)
            {
                VideoCourse? course = await _videoCourseService.GetCourseForVideo(model.VideoId);
                var authResult = await _authorizationService.AuthorizeAsync(
                    _userContext.User,
                    course.CourseId,
                    new CanModerateCommentInCourseRequirement()
                    );  

                if (!authResult.Succeeded &&  model.UserId != userId)
                {
                    throw new UnauthorizedAccessException();
                }

                await _reactionService.DeleteReactionsForCommentAsync(id);
                await _unitOfWork.Comments.DeleteChildren(id);
                await _unitOfWork.Comments.DeleteAsync(model);
                await _unitOfWork.SaveChangesAsync();
            }

        }

        
        public async Task<List<GetCommentDTO>> GetAllForVideo(Guid VideoId)
        {
            Guid? UserId = _userContext.UserId;
            VideoCourse? course = await _videoCourseService.GetCourseForVideo(VideoId);
            var authResult = await _authorizationService.AuthorizeAsync(
                _userContext.User,
                course.CourseId,
                new CanSeeCommentsInCourseRequirement()
                );
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
