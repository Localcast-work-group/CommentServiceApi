namespace CommentService.Api.Interfaces.Services
{
    public interface ICoursePermissionsService
    {
        Task CacheCommentAsync(Guid userId, Guid courseId);
        Task CacheModerateAsync(Guid userId, Guid courseId);
        Task CacheAccessAsync(Guid userId, Guid courseId);
        Task InvalidateCommentAsync(Guid userId, Guid courseId);
        Task InvalidateModerateAsync(Guid userId, Guid courseId);
        Task InvalidateAccessAsync(Guid userId, Guid courseId);
        Task<bool> CanCommentAsync(Guid userId, Guid courseId);
        Task<bool> CanModerateAsync(Guid userId, Guid courseId);
        Task<bool> HasAccessAsync(Guid userId, Guid courseId);
    }
}
