using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CommentService.Api.Authorization.Requiremments;
using CommentService.Api.Interfaces.Services;

namespace CommentService.Api.Authorization.Handlers
{
    public class CanSeeCommentsInCourseHandler : AuthorizationHandler<CanSeeCommentsInCourseRequirement, Guid>
    {
        private readonly ICoursePermissionsService _coursePermissionService;
        public CanSeeCommentsInCourseHandler(ICoursePermissionsService coursePermissionService)
        {
            _coursePermissionService = coursePermissionService;
        }
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CanSeeCommentsInCourseRequirement requirement,
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
            bool hasAccess = await _coursePermissionService.HasAccessAsync(Guid.Parse(userIdString), resource);

            if (!hasAccess)
            {
                context.Succeed(requirement);
                await Task.CompletedTask;
            }


            await Task.CompletedTask;
        }
    }
}
