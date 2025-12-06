using CommentService.Api.Consumers;
using CommentService.Api.Interfaces.Services;
using CourseService.Contracts.Events;
using MassTransit;
using Moq;
using FluentAssertions;

namespace CommentService.UnitTests.Consumers
{
    public class UserCoursePermissionsUpdatedConsumerTests
    {
        private readonly Mock<ICoursePermissionsService> _permissionsServiceMock;
        private readonly Mock<Serilog.ILogger> _loggerMock;
        private readonly Mock<ConsumeContext<UserCoursePermissionsUpdatedEvent>> _contextMock;

        private readonly UserCoursePermissionsUpdatedConsumer _consumer;

        public UserCoursePermissionsUpdatedConsumerTests()
        {
            _permissionsServiceMock = new Mock<ICoursePermissionsService>();
            _loggerMock = new Mock<Serilog.ILogger>();
            _contextMock = new Mock<ConsumeContext<UserCoursePermissionsUpdatedEvent>>();

            _consumer = new UserCoursePermissionsUpdatedConsumer(
                _loggerMock.Object,
                _permissionsServiceMock.Object
            );
        }

        [Fact]
        public async Task Consume_ShouldCacheAllPermissions_WhenAllFlagsAreTrue()
        {
            // 1. ARRANGE
            var message = new UserCoursePermissionsUpdatedEvent
            {
                UserId = Guid.NewGuid(),
                CourseId = Guid.NewGuid(),
                CanManage = true,
                CanWatch = true,
                CanComment = true,
                CanModerate = true
            };

            _contextMock.Setup(x => x.Message).Returns(message);

            // 2. ACT
            await _consumer.Consume(_contextMock.Object);

            // 3. ASSERT
            _permissionsServiceMock.Verify(x => x.CacheAccessAsync(message.UserId, message.CourseId), Times.Once);
            _permissionsServiceMock.Verify(x => x.CacheCommentAsync(message.UserId, message.CourseId), Times.Once);
            _permissionsServiceMock.Verify(x => x.CacheModerateAsync(message.UserId, message.CourseId), Times.Once);

            _permissionsServiceMock.Verify(x => x.InvalidateAccessAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _permissionsServiceMock.Verify(x => x.InvalidateCommentAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
            _permissionsServiceMock.Verify(x => x.InvalidateModerateAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Consume_ShouldInvalidateAll_WhenAllFlagsAreFalse()
        {
            // 1. ARRANGE
            var message = new UserCoursePermissionsUpdatedEvent
            {
                UserId = Guid.NewGuid(),
                CourseId = Guid.NewGuid(),
                CanWatch = false,
                CanComment = false,
                CanModerate = false
            };

            _contextMock.Setup(x => x.Message).Returns(message);

            // 2. ACT
            await _consumer.Consume(_contextMock.Object);

            // 3. ASSERT
            _permissionsServiceMock.Verify(x => x.InvalidateAccessAsync(message.UserId, message.CourseId), Times.Once);
            _permissionsServiceMock.Verify(x => x.InvalidateCommentAsync(message.UserId, message.CourseId), Times.Once);
            _permissionsServiceMock.Verify(x => x.InvalidateModerateAsync(message.UserId, message.CourseId), Times.Once);

            _permissionsServiceMock.Verify(x => x.CacheAccessAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Consume_ShouldMixActions_WhenFlagsAreMixed()
        {
            // 1. ARRANGE
            var message = new UserCoursePermissionsUpdatedEvent
            {
                UserId = Guid.NewGuid(),
                CourseId = Guid.NewGuid(),
                CanWatch = true,
                CanComment = true,
                CanModerate = false
            };

            _contextMock.Setup(x => x.Message).Returns(message);

            // 2. ACT
            await _consumer.Consume(_contextMock.Object);

            // 3. ASSERT
            _permissionsServiceMock.Verify(x => x.CacheAccessAsync(message.UserId, message.CourseId), Times.Once);
            _permissionsServiceMock.Verify(x => x.CacheCommentAsync(message.UserId, message.CourseId), Times.Once);

            _permissionsServiceMock.Verify(x => x.InvalidateModerateAsync(message.UserId, message.CourseId), Times.Once);
        }

        [Fact]
        public async Task Consume_ShouldRethrowException_WhenServiceFails()
        {
            // 1. ARRANGE
            var message = new UserCoursePermissionsUpdatedEvent { CanWatch = true };
            _contextMock.Setup(x => x.Message).Returns(message);

            _permissionsServiceMock.Setup(x => x.CacheAccessAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("Cache failure"));

            // 2. ACT
            Func<Task> action = async () => await _consumer.Consume(_contextMock.Object);

            // 3. ASSERT
            await action.Should().ThrowAsync<Exception>().WithMessage("Cache failure");
        }
    }
}