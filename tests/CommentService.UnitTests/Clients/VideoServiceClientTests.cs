using System.Net;
using System.Text.Json;
using CommentService.Api.Clients;
using FluentAssertions;
using Moq;
using Moq.Contrib.HttpClient;
using VideoService.Contracts.ApiResponses;

namespace CommentService.UnitTests.Clients
{
    public class VideoServiceClientTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly Mock<Serilog.ILogger> _loggerMock;
        private readonly VideoServiceClient _client;
        private readonly string _baseAddress = "http://fake-video-service.com/";

        public VideoServiceClientTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _loggerMock = new Mock<Serilog.ILogger>();

            var httpClient = _handlerMock.CreateClient();
            httpClient.BaseAddress = new Uri(_baseAddress);

            _client = new VideoServiceClient(httpClient, _loggerMock.Object);
        }

        [Fact]
        public async Task GetVideoCourseAsync_ShouldReturnResponse_WhenApiReturns200AndValidJson()
        {
            // 1. ARRANGE
            var videoId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            var requestUri = $"{_baseAddress}api/internal/video/{videoId}/courseId";

            var fakeResponse = new VideoCourseResponse
            {
                VideoId = videoId,
                CourseId = courseId
            };

            string jsonResponse = JsonSerializer.Serialize(fakeResponse);

            _handlerMock.SetupRequest(HttpMethod.Get, requestUri)
                        .ReturnsResponse(HttpStatusCode.OK, jsonResponse, "application/json");

            // 2. ACT
            var result = await _client.GetVideoCourseAsync(videoId);

            // 3. ASSERT
            result.Should().NotBeNull();
            result.VideoId.Should().Be(videoId);
            result.CourseId.Should().Be(courseId);
        }

        [Fact]
        public async Task GetVideoCourseAsync_ShouldReturnNull_WhenApiReturns404()
        {
            // 1. ARRANGE
            var videoId = Guid.NewGuid();
            var requestUri = $"{_baseAddress}api/internal/video/{videoId}/courseId";

            _handlerMock.SetupRequest(HttpMethod.Get, requestUri)
                        .ReturnsResponse(HttpStatusCode.NotFound);

            // 2. ACT
            var result = await _client.GetVideoCourseAsync(videoId);

            // 3. ASSERT
            result.Should().BeNull();

        }

        [Fact]
        public async Task GetVideoCourseAsync_ShouldReturnNull_WhenNetworkExceptionOccurs()
        {
            // 1. ARRANGE
            var videoId = Guid.NewGuid();
            var requestUri = $"{_baseAddress}api/internal/video/{videoId}/courseId";

            _handlerMock.SetupRequest(HttpMethod.Get, requestUri)
                        .Throws(new HttpRequestException("Network error"));

            // 2. ACT
            var result = await _client.GetVideoCourseAsync(videoId);

            // 3. ASSERT
            result.Should().BeNull();
        }
    }
}