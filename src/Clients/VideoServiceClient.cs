using System.Text.Json;
using VideoService.Contracts.ApiResponses;
using VideoService.Contracts.Clients;

namespace CommentService.Api.Clients
{
    public class VideoServiceClient : IVideoServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly Serilog.ILogger _logger;
        public VideoServiceClient(HttpClient httpClient, Serilog.ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<VideoCourseResponse> GetVideoCourseAsync(Guid videoId)
        {
            var requestUri = $"api/internal/video/{videoId}/courseId";

            try
            {
                var response = await _httpClient.GetAsync(requestUri);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.Warning("Validation failed", videoId);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var contentStream = await response.Content.ReadAsStreamAsync();
                var validationData = await JsonSerializer.DeserializeAsync<VideoCourseResponse>(contentStream, options);

                return validationData;
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex, "Connection error {RequestUri}", videoId, requestUri);

                return null;
            }
        }
    }
}
