using CommentService.Api.Interfaces;
using CommentService.Api.Interfaces.Repositories; 
using CommentService.Api.Models.VideoCourse;
using CommentService.Api.Services;
using FluentAssertions;
using Moq;
using VideoService.Contracts.ApiResponses; 
using VideoService.Contracts.Clients;

namespace CommentService.UnitTests.Services
{
    public class VideoCourseServiceTests
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IVideoServiceClient> _videoServiceClientMock;
        private readonly Mock<Serilog.ILogger> _loggerMock;

        private readonly Mock<IVideoCourseRepository> _videoCourseRepoMock;

        private readonly VideoCourseService _service;

        public VideoCourseServiceTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _videoServiceClientMock = new Mock<IVideoServiceClient>();
            _loggerMock = new Mock<Serilog.ILogger>();
            _videoCourseRepoMock = new Mock<IVideoCourseRepository>();

            _uowMock.Setup(x => x.VideoCourses).Returns(_videoCourseRepoMock.Object);

            _service = new VideoCourseService(
                _uowMock.Object,
                _loggerMock.Object,
                _videoServiceClientMock.Object
            );
        }

        [Fact]
        public async Task CreateCourseVideo_ShouldAddEntityAndSaveChanges()
        {
            // 1. ARRANGE
            var videoId = Guid.NewGuid();
            var courseId = Guid.NewGuid();

            // 2. ACT
            await _service.CreateCourseVideo(videoId, courseId);

            // 3. ASSERT
            _videoCourseRepoMock.Verify(x => x.AddAsync(It.Is<VideoCourse>(vc =>
                vc.VideoId == videoId &&
                vc.CourseId == courseId
            )), Times.Once);

            _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseForVideo_ShouldReturnFromDb_WhenExistsLocally()
        {
            // 1. ARRANGE
            var videoId = Guid.NewGuid();
            var expectedCourse = new VideoCourse { VideoId = videoId, CourseId = Guid.NewGuid() };

            _videoCourseRepoMock.Setup(x => x.GetOneAsync(videoId))
                .ReturnsAsync(expectedCourse);

            // 2. ACT
            var result = await _service.GetCourseForVideo(videoId);

            // 3. ASSERT
            result.Should().BeEquivalentTo(expectedCourse);

            _videoServiceClientMock.Verify(x => x.GetVideoCourseAsync(It.IsAny<Guid>()), Times.Never);

            _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetCourseForVideo_ShouldFetchFromClientAndSave_WhenMissingLocally()
        {
            // 1. ARRANGE
            var videoId = Guid.NewGuid();
            var courseId = Guid.NewGuid();

            _videoCourseRepoMock.Setup(x => x.GetOneAsync(videoId))
                .ReturnsAsync((VideoCourse?)null);

            var externalResponse = new VideoCourseResponse
            {
                VideoId = videoId,
                CourseId = courseId
            };

            _videoServiceClientMock.Setup(x => x.GetVideoCourseAsync(videoId))
                .ReturnsAsync(externalResponse);

            // 2. ACT
            var result = await _service.GetCourseForVideo(videoId);

            // 3. ASSERT
            result.Should().NotBeNull();
            result!.VideoId.Should().Be(videoId);
            result.CourseId.Should().Be(courseId);

            _videoCourseRepoMock.Verify(x => x.AddAsync(It.Is<VideoCourse>(vc =>
                vc.VideoId == videoId &&
                vc.CourseId == courseId
            )), Times.Once);

            _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCourseForVideo_ShouldReturnNull_WhenMissingLocallyAndExternally()
        {
            // 1. ARRANGE
            var videoId = Guid.NewGuid();

            _videoCourseRepoMock.Setup(x => x.GetOneAsync(videoId)).ReturnsAsync((VideoCourse?)null);

            _videoServiceClientMock.Setup(x => x.GetVideoCourseAsync(videoId)).ReturnsAsync((VideoCourseResponse?)null);

            // 2. ACT
            var result = await _service.GetCourseForVideo(videoId);

            // 3. ASSERT
            result.Should().BeNull();

            _videoCourseRepoMock.Verify(x => x.AddAsync(It.IsAny<VideoCourse>()), Times.Never);
            _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}