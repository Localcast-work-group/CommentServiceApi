using CommentService.Api.Models.Comment.DTOs;

namespace CommentService.Api.Interfaces.Services
{
    public interface ICommentService
    {
        public Task<GetCommentDTO> Add(CreateCommentDTO createCommentDTO);
        public Task<List<GetCommentDTO>> GetAllForVideo(Guid VideoId);
        public Task Delete(Guid id);
    }
}
