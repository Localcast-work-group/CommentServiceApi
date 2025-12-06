using System.Security.Claims;

namespace CommentService.Api.Interfaces.Services
{
    public interface IUserContext
    {
        Guid? UserId { get; }
        ClaimsPrincipal User { get; }
    }
}
