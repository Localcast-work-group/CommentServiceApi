using CourseService.Contracts.Events;
using MassTransit;
using CommentService.Api.Interfaces.Services;

namespace CommentService.Api.Consumers
{
    public class UserCoursePermissionsUpdatedConsumer : IConsumer<UserCoursePermissionsUpdatedEvent>
    {
        private readonly Serilog.ILogger _logger;
        private readonly ICoursePermissionsService _coursePermissionsService;

        public UserCoursePermissionsUpdatedConsumer(
            Serilog.ILogger logger,
            ICoursePermissionsService coursePermissionsService)
        {
            _logger = logger;
            _coursePermissionsService = coursePermissionsService;
        }

        public async Task Consume(ConsumeContext<UserCoursePermissionsUpdatedEvent> context)
        {
            var message = context.Message;

            _logger.Information(
                "The UserCoursePermissionsUpdatedEvent event was received for CourseId {CourseId}",
                message.CourseId);

            try
            {
                if (message.CanModerate)
                {

                    await _coursePermissionsService.CacheModerateAsync(message.UserId,message.CourseId);
                    _logger.Information(
                        "User {UserId} can now Moderate course {CourseId}",
                        message.UserId, message.CourseId);
                }
                else
                {
                    await _coursePermissionsService.InvalidateModerateAsync(message.UserId, message.CourseId);
                    _logger.Information(
                        "User {UserId} Moderate access removed for course {CourseId}",
                        message.UserId, message.CourseId);
                }
                if(message.CanWatch)
                {
                    await _coursePermissionsService.CacheAccessAsync(message.UserId,message.CourseId);
                    _logger.Information(
                        "User {UserId} can now watch comment's course {CourseId}",
                        message.UserId, message.CourseId);
                }
                else
                {
                    await _coursePermissionsService.InvalidateAccessAsync(message.UserId,message.CourseId );
                    _logger.Information(
                        "User {UserId} comment's access removed for course {CourseId}",
                        message.UserId, message.CourseId);
                }
                if (message.CanComment)
                {
                    await _coursePermissionsService.CacheCommentAsync(message.UserId, message.CourseId);
                    _logger.Information(
                        "User {UserId} can now comment course {CourseId}",
                        message.UserId, message.CourseId);
                }
                else
                {
                    await _coursePermissionsService.InvalidateCommentAsync(message.UserId, message.CourseId);
                    _logger.Information(
                        "User {UserId} comment permission removed for course {CourseId}",
                        message.UserId, message.CourseId);
                }

            }
            catch (Exception ex)
            {
                _logger.Information(ex,
                    "Error updating access for course {CourseId}",
                    message.CourseId);

                throw;
            }
        }
    }
}
    
