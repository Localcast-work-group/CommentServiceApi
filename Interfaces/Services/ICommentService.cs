using CommentService.Api.Models.Comment.DTOs;

namespace CommentService.Api.Interfaces.Services
{
    public interface ICommentService
    {
        public Task<GetCommentDTO> Add(CreateCommentDTO createCommentDTO, Guid userId, string userName);
        public Task<List<GetCommentDTO>> GetAllForVideo(Guid VideoId,Guid? userId);
        public Task Delete(Guid id, Guid? userId = null);
    }
}
