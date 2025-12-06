using CommentService.Api.Interfaces.Services;
using CommentService.Api.Models.Comment.DTOs;
using CommentService.Api.Enums;
using CommentService.Api.Models.Reaction.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Api.Controllers
{
    [ApiController]
    [Authorize]
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
            
            List<GetCommentDTO> comments = await _commentService.GetAllForVideo(VideoId);
            

            return Ok(comments);
        }
        [HttpPost("", Name = "CreateComment")]

        public async Task<IActionResult> Post(CreateCommentDTO model)
        {

            GetCommentDTO comment  = await _commentService.Add(model);
                return Ok(
                   new  { response = comment });
            

        }
        [HttpDelete("{Id}", Name = "DeleteComment")]

        public async Task<IActionResult> Delete(Guid Id)
        {
                
            await _commentService.Delete(Id);
            return Ok();

        }
        [HttpPost("ToggleReaction", Name = "ToggleReaction")]

        public async Task<IActionResult> ToggleReaction(ToggleReactionDTO model)
        {

            await _reactionService.ToggleReaction(model.TargetId, (ToggleReactionEnum)model.ToggleType);
            return Ok();
            

        }
    }
}
