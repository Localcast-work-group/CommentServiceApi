using CategoryService.Api.Interfaces.Services;
using MassTransit;
using VideoService.Contracts.Events;

namespace CategoryService.Api.Consumers
{
    public class VideoCreatedEventConsumer : IConsumer<VideoCreatedEvent>
    {
        private readonly Serilog.ILogger _logger;
        private readonly ICategoryService _categoryService;

        public VideoCreatedEventConsumer(
            Serilog.ILogger logger,
            ICategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        public async Task Consume(ConsumeContext<VideoCreatedEvent> context)
        {
            var message = context.Message;

            _logger.Information(
                "The VideoCreatedEvent event was received for CategoryId {CategoryId}",
                message.CategoryId);

            try
            {
                if(message.Visible)
                {

                    await _categoryService.IncrementVideoCount(message.CategoryId);
                    _logger.Information(
                        "The counter for the {CategoryId} category has been successfully updated",
                        message.CategoryId);
                }

            }
            catch (Exception ex)
            {
                _logger.Information(ex,
                    "Error updating counter for category {CategoryId}",
                    message.CategoryId);

                throw;
            }
        }
    }
}