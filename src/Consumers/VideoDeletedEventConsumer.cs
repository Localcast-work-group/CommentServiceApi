using CommentService.Api.Interfaces.Services;
using MassTransit;
using VideoService.Contracts.Events;

namespace CommentService.Api.Consumers
{
    public class VideoDeletedEventConsumer : IConsumer<VideoDeletedEvent>
    {
        private readonly Serilog.ILogger _logger;
        private readonly ICommentService _commentService;
        private readonly IVideoCourseService _videoCourseService;

        public VideoDeletedEventConsumer(
            Serilog.ILogger logger,
            ICommentService commentService, IVideoCourseService videoCourseService)
        {
            _logger = logger;
            _commentService = commentService;
            _videoCourseService = videoCourseService;
        }
        public async Task Consume(ConsumeContext<VideoDeletedEvent> context)
        {
            var message = context.Message;
            _logger.Information(
                "The VideoDeletedEvent event was received for VideoId {VideoId}",
                message.Id);
            try
            {
                await _commentService.DeleteAllForVideo(message.Id);
                _logger.Information(
                    "Video {VideoId} comments deleted",
                    message.Id);
                await _videoCourseService.DeleteCourseVideo(message.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Error processing VideoDeletedEvent for VideoId {VideoId}",
                    message.Id);
            }
        }
    }


}
