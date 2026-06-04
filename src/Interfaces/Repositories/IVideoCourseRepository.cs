using CommentService.Api.Models.VideoCourse;

namespace CommentService.Api.Interfaces.Repositories
{
    public interface IVideoCourseRepository
    {
        Task AddAsync(VideoCourse model);
        void Delete(VideoCourse model);
        Task<VideoCourse?> GetOneAsync(Guid videoId);
    }
}