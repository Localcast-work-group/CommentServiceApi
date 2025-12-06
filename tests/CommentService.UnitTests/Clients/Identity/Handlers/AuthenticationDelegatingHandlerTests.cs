using CommentService.Api.Clients.Handlers;
using CommentService.Api.Interfaces.Services;
using Moq;
using Moq.Protected;
using System.Net;

namespace CommentService.UnitTests.Clients.Identity.Handlers
{
    public class AuthenticationDelegatingHandlerTests
    {
        [Fact]
        public async Task SendAsync_ShouldAddAuthorizationHeader_WhenRequestIsSent()
        {
            // 1. ARRANGE
            var testToken = "super_secret_token_123";

            var tokenServiceMock = new Mock<IIdentityTokenService>();
            tokenServiceMock.Setup(x => x.GetTokenAsync())
                .ReturnsAsync(testToken);

            var authHandler = new AuthenticationDelegatingHandler(tokenServiceMock.Object);

            var innerHandlerMock = new Mock<HttpMessageHandler>();

            innerHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            authHandler.InnerHandler = innerHandlerMock.Object;

            var invoker = new HttpMessageInvoker(authHandler);

            // 2. ACT
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/api/resource");
            await invoker.SendAsync(request, CancellationToken.None);

            // 3. ASSERT
            innerHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.Authorization != null &&
                    req.Headers.Authorization.Scheme == "Bearer" &&
                    req.Headers.Authorization.Parameter == testToken 
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
