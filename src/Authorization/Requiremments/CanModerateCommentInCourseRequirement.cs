using Microsoft.AspNetCore.Authorization;

namespace CommentService.Api.Authorization.Requiremments
{
    public class CanModerateCommentInCourseRequirement : IAuthorizationRequirement
    {
    }
}
