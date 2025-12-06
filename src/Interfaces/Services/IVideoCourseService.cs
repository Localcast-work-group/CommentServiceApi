using CommentService.Api.Models.VideoCourse;

namespace CommentService.Api.Interfaces.Services
{
    public interface IVideoCourseService
    {
        Task<VideoCourse> GetCourseForVideo(Guid videoId);
        Task CreateCourseVideo (Guid videoId, Guid courseId);
    }
}