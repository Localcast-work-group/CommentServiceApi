using CommentService.Api.Clients.Identity;
using CommentService.Api.Interfaces.Services;
using System.Net.Http.Headers;

namespace CommentService.Api.Clients.Handlers
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IIdentityTokenService _tokenService;

        public AuthenticationDelegatingHandler(IIdentityTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenService.GetTokenAsync();

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}