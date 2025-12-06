using CommentService.Api.Interfaces.Services;
using MassTransit;
using VideoService.Contracts.Events;

namespace CommentService.Api.Consumers
{
    public class VideoCreatedEventConsumer : IConsumer<VideoCreatedEvent>
    {
        private readonly Serilog.ILogger _logger;
        private readonly IVideoCourseService _videoCourseService;
        public VideoCreatedEventConsumer(
            Serilog.ILogger logger,
            IVideoCourseService videoCourseService)
        {
            _logger = logger;
            _videoCourseService = videoCourseService;
        }
        public async Task Consume(ConsumeContext<VideoCreatedEvent> context)
        {
            var message = context.Message;
            _logger.Information(
                "The VideoCreatedEvent event was received for VideoId {VideoId}",
                message.Id);
            try
            {
                await _videoCourseService.CreateCourseVideo(message.Id,message.CourseId);
                _logger.Information(
                    "Video {VideoId} course cached",
                    message.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Error processing VideoCreatedEvent for VideoId {VideoId}",
                    message.Id);
            }
        }
    }
}
