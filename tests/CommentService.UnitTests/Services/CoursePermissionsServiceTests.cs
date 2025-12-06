using System.Text;
using CommentService.Api.Services;
using CourseService.Contracts.ApiResponses;
using CourseService.Contracts.Clients;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace CommentService.UnitTests.Services
{
    public class CoursePermissionsServiceTests
    {
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<ICourseServiceClient> _courseClientMock;
        private readonly Mock<Serilog.ILogger> _loggerMock;

        private readonly CoursePermissionsService _service;
        private readonly Guid _userId = Guid.NewGuid();
        private readonly Guid _courseId = Guid.NewGuid();

        public CoursePermissionsServiceTests()
        {
            _cacheMock = new Mock<IDistributedCache>();
            _courseClientMock = new Mock<ICourseServiceClient>();
            _loggerMock = new Mock<Serilog.ILogger>();

            _service = new CoursePermissionsService(
                _cacheMock.Object,
                _courseClientMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CanCommentAsync_ShouldReturnTrueFromCache_WithoutCallingClient()
        {
            // 1. ARRANGE
            var cachedValue = Encoding.UTF8.GetBytes("true");
            _cacheMock.Setup(x => x.GetAsync($"comment:{_userId}:{_courseId}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedValue);

            // 2. ACT
            var result = await _service.CanCommentAsync(_userId, _courseId);

            // 3. ASSERT
            result.Should().BeTrue();
            _courseClientMock.Verify(x => x.CheckUserPermissionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task CanCommentAsync_ShouldCallClient_AndCacheAllPermissions_WhenCacheMiss()
        {
            // 1. ARRANGE
            _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            var permissions = new UserCoursePermissionsResponse
            {
                CanComment = true,
                CanWatch = true,
                CanModerate = true
            };

            _courseClientMock.Setup(x => x.CheckUserPermissionsAsync(_courseId, _userId))
                .ReturnsAsync(permissions);

            // 2. ACT
            var result = await _service.CanCommentAsync(_userId, _courseId);

            // 3. ASSERT
            result.Should().BeTrue();

            _cacheMock.Verify(x => x.SetAsync($"comment:{_userId}:{_courseId}", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(x => x.SetAsync($"access:{_userId}:{_courseId}", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(x => x.SetAsync($"moderate:{_userId}:{_courseId}", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CanModerateAsync_ShouldReturnFalse_WhenClientReturnsFalse_AndCacheMiss()
        {
            // 1. ARRANGE
            _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            var permissions = new UserCoursePermissionsResponse { CanModerate = false };
            _courseClientMock.Setup(x => x.CheckUserPermissionsAsync(_courseId, _userId))
                .ReturnsAsync(permissions);

            // 2. ACT
            var result = await _service.CanModerateAsync(_userId, _courseId);

            // 3. ASSERT
            result.Should().BeFalse();
            _cacheMock.Verify(x => x.SetAsync($"moderate:{_userId}:{_courseId}", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HasAccessAsync_ShouldReturnFalse_WhenClientThrowsException()
        {
            // 1. ARRANGE
            _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            _courseClientMock.Setup(x => x.CheckUserPermissionsAsync(_courseId, _userId))
                .ThrowsAsync(new Exception("API Error"));

            // 2. ACT
            var result = await _service.HasAccessAsync(_userId, _courseId);

            // 3. ASSERT
            result.Should().BeFalse();
        }

        [Fact]
        public async Task InvalidateMethods_ShouldRemoveCorrectKeys()
        {

            // 1. ACT
            await _service.InvalidateAccessAsync(_userId, _courseId);
            await _service.InvalidateCommentAsync(_userId, _courseId);
            await _service.InvalidateModerateAsync(_userId, _courseId);

            // 2. ASSERT
            _cacheMock.Verify(x => x.RemoveAsync($"access:{_userId}:{_courseId}", It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(x => x.RemoveAsync($"comment:{_userId}:{_courseId}", It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(x => x.RemoveAsync($"moderate:{_userId}:{_courseId}", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CacheMethods_ShouldSetCorrectKeys()
        {

            // 1. ACT
            await _service.CacheAccessAsync(_userId, _courseId);
            await _service.CacheCommentAsync(_userId, _courseId);
            await _service.CacheModerateAsync(_userId, _courseId);

            // 2. ASSERT
            _cacheMock.Verify(x => x.SetAsync($"access:{_userId}:{_courseId}", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(x => x.SetAsync($"comment:{_userId}:{_courseId}", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(x => x.SetAsync($"moderate:{_userId}:{_courseId}", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}