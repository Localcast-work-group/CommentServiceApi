using CommentService.Api.Interfaces.Services;
using MassTransit;
using VideoService.Contracts.Events;

namespace CommentService.Api.Consumers
{
    public class VideoUpdatedEventConsumer : IConsumer<VideoUpdatedEvent>
    {
        private readonly Serilog.ILogger _logger;
        private readonly IVideoCourseService _videoCourseService;
        public VideoUpdatedEventConsumer(
            Serilog.ILogger logger,
            IVideoCourseService videoCourseService)
        {
            _logger = logger;
            _videoCourseService = videoCourseService;
        }
        public async Task Consume(ConsumeContext<VideoUpdatedEvent> context)
        {
            var message = context.Message;
            _logger.Information(
                "The VideoUpdatedEvent event was received for VideoId {VideoId}",
                message.Id);
            try
            {
                await _videoCourseService.UpdateCourseVideo(message.Id, message.CourseId, message.AllowAnonymous);
                _logger.Information(
                    "Video {VideoId} course cached",
                    message.Id);

            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Error processing VideoUpdatedEvent for VideoId {VideoId}",
                    message.Id);
            }
        }
    }
}
