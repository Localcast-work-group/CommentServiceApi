namespace CommentService.Api.Interfaces.Services
{
    public interface IJwtService
    {
        public Task<(string JwtToken, string RefreshToken)> GenerateTokens(Guid userId);
        public Task<bool> ValidateRefreshToken(Guid userId, string refreshToken);
        public Task RevokeRefreshToken(string refreshToken);

    }
}
