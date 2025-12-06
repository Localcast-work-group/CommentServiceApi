using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CommentService.Api.Authorization.Requiremments;
using CommentService.Api.Interfaces.Services;

namespace CommentService.Api.Authorization.Handlers
{
    public class CanModerateCommentInCourseHandler : AuthorizationHandler<CanModerateCommentInCourseRequirement, Guid>
    {
        private readonly ICoursePermissionsService _coursePermissionService;

        public CanModerateCommentInCourseHandler(ICoursePermissionsService coursePermissionService)
        {
            _coursePermissionService = coursePermissionService;
        }
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CanModerateCommentInCourseRequirement requirement,
            Guid resource)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                await Task.CompletedTask;
            }


            var userIdString = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                await Task.CompletedTask;
            }
            bool canManage = await _coursePermissionService.CanModerateAsync(Guid.Parse(userIdString),resource);

            if (!canManage)
            {
                context.Succeed(requirement);
                await Task.CompletedTask;
            }


            await Task.CompletedTask;
        }
    }
}
