using CommentService.Api.Interfaces.Services;
using CommentService.Api.Models.Comment.DTOs;
using CommentService.Api.Enums;
using CommentService.Api.Models.Reaction.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CommentService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IReactionService _reactionService;
        private readonly IWebHostEnvironment _environment;

        public CommentController(ICommentService commentService , IReactionService reactionService)
        {
            _commentService = commentService;
            _reactionService = reactionService;
        }
        [HttpGet("{VideoId}", Name = "GetComments")]

        public async Task<IActionResult> Get(Guid VideoId)
        {
            Guid? userId = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                userId = Guid.Parse((User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()).Value);
            }
            List<GetCommentDTO> comments = await _commentService.GetAllForVideo(VideoId, userId);
            

            return Ok(comments);
        }
        [HttpPost("", Name = "CreateComment")]

        [Authorize]
        public async Task<IActionResult> Post(CreateCommentDTO model)
        {
            try
            {
                Guid userId = Guid.Parse((User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()).Value);
                string userName = (User.Claims.Where(x => x.Type == ClaimTypes.Name).FirstOrDefault().Value).ToString();

               GetCommentDTO comment  = await _commentService.Add(model,userId,userName);
                return Ok(
                   new  { response = comment });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
        [HttpDelete("{Id}", Name = "DeleteComment")]

        [Authorize]
        public async Task<IActionResult> Delete(Guid Id)
        {
            try
            {
                Guid userId = Guid.Parse((User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()).Value);
                try
                {
                    await _commentService.Delete(userId,Id);

                }
                catch (UnauthorizedAccessException ex) 
                {
                    return Unauthorized();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
        [HttpPost("ToggleReaction", Name = "ToggleReaction")]

        [Authorize]
        public async Task<IActionResult> ToggleReaction(ToggleReactionDTO model)
        {
            try
            {
                Guid userId = Guid.Parse((User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault()).Value);

                await _reactionService.ToggleReaction(userId, model.TargetId, (ToggleReactionEnum)model.ToggleType);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }
    }
}
