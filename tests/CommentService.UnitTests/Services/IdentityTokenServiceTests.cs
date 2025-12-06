using CommentService.Api.Clients;
using CommentService.Api.Clients.Identity.Configuration;
using FluentAssertions;
using IdentityService.Contracts.ApiResponses;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Contrib.HttpClient;
using System.Net;
using System.Text.Json;
namespace CommentService.UnitTests.Services
{
    public class IdentityTokenServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpHandlerMock;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<IOptions<IdentitySettings>> _settingsMock;
        private readonly Mock<Serilog.ILogger> _loggerMock;
        private readonly IdentityTokenService _service;

        public IdentityTokenServiceTests()
        {
            _httpHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = _httpHandlerMock.CreateClient();

            _memoryCache = new MemoryCache(new MemoryCacheOptions());

            _settingsMock = new Mock<IOptions<IdentitySettings>>();
            _settingsMock.Setup(x => x.Value).Returns(new IdentitySettings
            {
                Authority = "http://auth-server",
                ClientId = "client_id",
                ClientSecret = "secret"
            });

            _loggerMock = new Mock<Serilog.ILogger>();

            _service = new IdentityTokenService(httpClient, _settingsMock.Object, _memoryCache, _loggerMock.Object);
        }

        [Fact]
        public async Task GetTokenAsync_ShouldReturnTokenFromApi_WhenCacheIsEmpty()
        {
            // 1. ARRANGE
            var tokenResponse = new TokenResponse { AccessToken = "new_fresh_token", ExpiresIn = 3600, TokenType="Bearer" };
            var json = JsonSerializer.Serialize(tokenResponse);

            _httpHandlerMock.SetupRequest(HttpMethod.Post, "http://auth-server/api/oauth/token")
                .ReturnsResponse(HttpStatusCode.OK, json, "application/json");

            // 2. ACT
            var result = await _service.GetTokenAsync();

            // 3. ASSERT
            result.Should().Be("new_fresh_token");

            _httpHandlerMock.VerifyRequest(HttpMethod.Post, "http://auth-server/api/oauth/token", Times.Once());
        }

        [Fact]
        public async Task GetTokenAsync_ShouldReturnCachedToken_WithoutCallingApi_WhenTokenIsInCache()
        {
            // 1. ARRANGE
            _memoryCache.Set("IdentityService_Token", "cached_token_123");

            _httpHandlerMock.SetupRequest(HttpMethod.Post, "http://auth-server/api/oauth/token")
                .Throws(new Exception("Should not be called!"));

            // 2. ACT
            var result = await _service.GetTokenAsync();

            // 3. ASSERT
            result.Should().Be("cached_token_123");

            _httpHandlerMock.VerifyRequest(HttpMethod.Post, "http://auth-server/api/oauth/token", Times.Never());
        }
    }
}