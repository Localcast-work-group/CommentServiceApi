namespace CommentService.Api.Interfaces.Services
{
    public interface IIdentityTokenService
    {
        Task<string> GetTokenAsync();
    }
}