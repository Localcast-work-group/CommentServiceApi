using System;
using System.Threading.Tasks;
using CommentService.Api.Consumers;
using CommentService.Api.Interfaces.Services;
using MassTransit;
using Moq;
using VideoService.Contracts.Events;
using Xunit;

namespace CommentService.UnitTests.Consumers
{
    public class VideoCreatedEventConsumerTests
    {
        private readonly Mock<IVideoCourseService> _videoCourseServiceMock;
        private readonly Mock<Serilog.ILogger> _loggerMock;
        private readonly Mock<ConsumeContext<VideoCreatedEvent>> _contextMock;

        private readonly VideoCreatedEventConsumer _consumer;

        public VideoCreatedEventConsumerTests()
        {
            _videoCourseServiceMock = new Mock<IVideoCourseService>();
            _loggerMock = new Mock<Serilog.ILogger>();
            _contextMock = new Mock<ConsumeContext<VideoCreatedEvent>>();

            _consumer = new VideoCreatedEventConsumer(
                _loggerMock.Object,
                _videoCourseServiceMock.Object
            );
        }

        [Fact]
        public async Task Consume_ShouldCallCreateCourseVideo_WhenEventIsValid()
        {
            // 1. ARRANGE
            var message = new VideoCreatedEvent
            {
                Id = Guid.NewGuid(),       
                CourseId = Guid.NewGuid(), 
                UserId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Visible = true
            };

            _contextMock.Setup(x => x.Message).Returns(message);

            // 2. ACT
            await _consumer.Consume(_contextMock.Object);

            // 3. ASSERT
            _videoCourseServiceMock.Verify(x => x.CreateCourseVideo(message.Id, message.CourseId), Times.Once);
        }

        [Fact]
        public async Task Consume_ShouldSwallowException_WhenServiceFails()
        {

            // 1. ARRANGE
            var message = new VideoCreatedEvent
            {
                Id = Guid.NewGuid(),
                CourseId = Guid.NewGuid()
            };
            _contextMock.Setup(x => x.Message).Returns(message);

            _videoCourseServiceMock.Setup(x => x.CreateCourseVideo(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Database error"));

            // 2. ACT
         
            var exception = await Record.ExceptionAsync(() => _consumer.Consume(_contextMock.Object));
            // 3. ASSERT
            Assert.Null(exception); 

            _videoCourseServiceMock.Verify(x => x.CreateCourseVideo(message.Id, message.CourseId), Times.Once);
        }
    }
}