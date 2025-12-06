using CommentService.Api.Authorization;
using CommentService.Api.Authorization.Handlers;
using Microsoft.AspNetCore.Authorization;

namespace CommentService.Api.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizationHandler, CanSeeCommentsInCourseHandler>();
            services.AddScoped<IAuthorizationHandler, CanAddCommentInCourseHandler>();
            services.AddScoped<IAuthorizationHandler, CanModerateCommentInCourseHandler>();
            return services;
        }
    }
}


