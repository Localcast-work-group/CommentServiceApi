using CommentService.Api.Clients.Identity.Configuration;
using CommentService.Api.Interfaces.Services;
using IdentityService.Contracts.ApiResponses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CommentService.Api.Clients
{
    public class IdentityTokenService : IIdentityTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IdentitySettings _settings;
        private readonly IMemoryCache _cache;
        private readonly Serilog.ILogger _logger;

        public IdentityTokenService(HttpClient httpClient, IOptions<IdentitySettings> settings, IMemoryCache cache, Serilog.ILogger logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _cache = cache;
            _logger = logger;
        }

        public async Task<string> GetTokenAsync()
        {
            if (_cache.TryGetValue("IdentityService_Token", out string cachedToken))
            {
                return cachedToken;
            }

            _logger.Information("Requesting new Access Token for IdentityService...");

            var request = new
            {
                _settings.ClientId,
                _settings.ClientSecret
            };

            var response = await _httpClient.PostAsJsonAsync($"{_settings.Authority}/api/oauth/token", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();

            if (result?.AccessToken == null)
            {
                throw new Exception("Failed to retrieve access token from IdentityService.");
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(result.ExpiresIn - 60));

            _cache.Set("IdentityService_Token", result.AccessToken, cacheEntryOptions);

            return result.AccessToken;
        }

    }
}