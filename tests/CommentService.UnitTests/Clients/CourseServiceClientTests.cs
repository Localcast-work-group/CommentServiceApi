using System.Net;
using System.Text.Json;
using CommentService.Api.Clients;
using CourseService.Contracts.ApiResponses;
using FluentAssertions;
using Moq;
using Moq.Contrib.HttpClient;

namespace CommentService.UnitTests.Clients
{
    public class CourseServiceClientTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly Mock<Serilog.ILogger> _loggerMock;
        private readonly CourseServiceClient _client;
        private readonly string _baseAddress = "http://fake-course-service.com/";

        public CourseServiceClientTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _loggerMock = new Mock<Serilog.ILogger>();

            var httpClient = _handlerMock.CreateClient();
            httpClient.BaseAddress = new Uri(_baseAddress);

            _client = new CourseServiceClient(httpClient, _loggerMock.Object);
        }

        [Fact]
        public async Task CheckUserPermissionsAsync_ShouldReturnPermissions_WhenApiReturns200AndValidJson()
        {
            // 1. ARRANGE
            var userId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            var requestUri = $"{_baseAddress}api/internal/course/{courseId}/permissions/{userId}";

            var fakeResponse = new UserCoursePermissionsResponse
            {
                UserId = userId,
                CourseId = courseId,
                CanComment = true,
                CanModerate = true,
                CanWatch = true
            };

            string jsonResponse = JsonSerializer.Serialize(fakeResponse);

            _handlerMock.SetupRequest(HttpMethod.Get, requestUri)
                        .ReturnsResponse(HttpStatusCode.OK, jsonResponse, "application/json");

            // 2. ACT
            var result = await _client.CheckUserPermissionsAsync(userId, courseId);

            // 3. ASSERT
            result.Should().NotBeNull();
            result.CanComment.Should().BeTrue();
            result.CanModerate.Should().BeTrue();
            result.CanWatch.Should().BeTrue();
            result.CourseId.Should().Be(courseId);
        }

        [Fact]
        public async Task CheckUserPermissionsAsync_ShouldReturnDefaultFalse_WhenApiReturns404()
        {
            // 1. ARRANGE
            var userId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            var requestUri = $"{_baseAddress}api/internal/course/{courseId}/permissions/{userId}";

            _handlerMock.SetupRequest(HttpMethod.Get, requestUri)
                        .ReturnsResponse(HttpStatusCode.NotFound);

            // 2. ACT
            var result = await _client.CheckUserPermissionsAsync(userId, courseId);

            // 3. ASSERT
            result.Should().NotBeNull();
            result.CanComment.Should().BeFalse();
            result.CanModerate.Should().BeFalse();
            result.CanWatch.Should().BeFalse();

            result.UserId.Should().Be(userId);
            result.CourseId.Should().Be(courseId);
        }

        [Fact]
        public async Task CheckUserPermissionsAsync_ShouldReturnDefaultFalse_WhenNetworkExceptionOccurs()
        {
            // 1. ARRANGE
            var userId = Guid.NewGuid();
            var courseId = Guid.NewGuid();
            var requestUri = $"{_baseAddress}api/internal/course/{courseId}/permissions/{userId}";

            _handlerMock.SetupRequest(HttpMethod.Get, requestUri)
                        .Throws(new HttpRequestException("Network connection failed"));

            // 2. ACT
            var result = await _client.CheckUserPermissionsAsync(userId, courseId);

            // 3. ASSERT
            result.Should().NotBeNull();
            result.CanComment.Should().BeFalse();
            result.CanWatch.Should().BeFalse();
            result.UserId.Should().Be(userId);
        }
    }
}