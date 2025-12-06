using CourseService.Contracts.ApiResponses;
using CourseService.Contracts.Clients;
using System.Text.Json;

namespace CommentService.Api.Clients
{
    public class CourseServiceClient : ICourseServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly Serilog.ILogger _logger;

        public CourseServiceClient(HttpClient httpClient, Serilog.ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<UserCoursePermissionsResponse> CheckUserPermissionsAsync(Guid userId, Guid courseId)
        {
            var requestUri = $"api/internal/course/{courseId}/permissions/{userId}";

            try
            {
                var response = await _httpClient.GetAsync(requestUri);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.Warning("Validation failed", userId);
                    return new UserCoursePermissionsResponse { CanComment = false, CanModerate = false, CanWatch = false, CourseId = courseId, UserId = userId };
                }

                response.EnsureSuccessStatusCode();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var contentStream = await response.Content.ReadAsStreamAsync();
                var validationData = await JsonSerializer.DeserializeAsync<UserCoursePermissionsResponse>(contentStream, options);

                return validationData ?? new UserCoursePermissionsResponse {CanComment = false, CanModerate = false, CanWatch = false, CourseId = courseId, UserId = userId };
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex, "Connection error {RequestUri}", userId, requestUri);

                return new UserCoursePermissionsResponse { CanComment = false, CanModerate = false, CanWatch = false, CourseId = courseId, UserId = userId };
            }
        }

    }
}
