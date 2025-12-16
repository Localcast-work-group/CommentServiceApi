using CommentService.Api.Authorization.Requiremments;
using CommentService.Api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CommentService.Api.Authorization.Handlers
{
    public class CanAddCommentInCourseHandler : AuthorizationHandler<CanAddCommentInCourseRequirement,Guid>
    {
        private readonly ICoursePermissionsService _coursePermissionService;
        public CanAddCommentInCourseHandler(ICoursePermissionsService coursePermissionService)
        {
            _coursePermissionService = coursePermissionService;
        }

        protected async override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, CanAddCommentInCourseRequirement requirement, Guid resource)
        {
            if (context.User.IsInRole("Admin") )
            {
                context.Succeed(requirement);
                await Task.CompletedTask;
                return;

            }
            var userIdString = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                await Task.CompletedTask;
                return;

            }
            bool canManage = await _coursePermissionService.CanCommentAsync(Guid.Parse(userIdString), resource);

            if (canManage)
            {
                context.Succeed(requirement);
                await Task.CompletedTask;
                return;

            }



            await Task.CompletedTask;
        }

    }
}
