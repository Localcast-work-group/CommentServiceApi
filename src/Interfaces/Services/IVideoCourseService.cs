using CommentService.Api.Models.VideoCourse;

namespace CommentService.Api.Interfaces.Services
{
    public interface IVideoCourseService
    {
        Task<VideoCourse> GetCourseForVideo(Guid videoId);
        Task UpdateCourseVideo (Guid videoId, Guid courseId, bool isAllowAnonymous);
        Task CreateCourseVideo (Guid videoId, Guid courseId, bool isAllowAnonymous);
        Task DeleteCourseVideo(Guid id);
    }
}