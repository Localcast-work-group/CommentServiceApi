using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CommentService.Api.Authorization.Requiremments;
using CommentService.Api.Interfaces.Services;
using CommentService.Api.Models.VideoCourse;

namespace CommentService.Api.Authorization.Handlers
{
    public class CanSeeCommentsInCourseHandler : AuthorizationHandler<CanSeeCommentsInCourseRequirement, VideoCourse>
    {
        private readonly ICoursePermissionsService _coursePermissionService;
        private readonly IVideoCourseService _videoCourseService;
        public CanSeeCommentsInCourseHandler(ICoursePermissionsService coursePermissionService, IVideoCourseService videoCourseService)
        {
            _coursePermissionService = coursePermissionService;
            _videoCourseService = videoCourseService;
        }
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            CanSeeCommentsInCourseRequirement requirement,
            VideoCourse resource)
        {
            if (resource.IsAllowAnonymousComments)
            {
                context.Succeed(requirement);
                await Task.CompletedTask;
                return;

            }
            if (context.User.IsInRole("Admin"))
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
            bool hasAccess = await _coursePermissionService.HasAccessAsync(Guid.Parse(userIdString), resource.CourseId);

            if (hasAccess)
            {
                context.Succeed(requirement);
                await Task.CompletedTask;
                return;

            }


            await Task.CompletedTask;
        }
    }
}
