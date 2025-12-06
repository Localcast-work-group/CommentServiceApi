using System.Security.Claims;
using CommentService.Api.Interfaces.Services;

namespace CommentService.Api.Services
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _accessor;
        public UserContext(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public ClaimsPrincipal User => _accessor.HttpContext?.User;

        public Guid? UserId
        {
            get
            {
                var val = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Guid.TryParse(val, out var id) ? id : null;
            }
        }
    }
}
