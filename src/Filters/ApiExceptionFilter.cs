using FluentValidation;
using CommentService.Api.Exceptions;
using CommentService.Api.Exceptions.BusinessRuleValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CommentService.Api.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        Serilog.ILogger Logger;

        public ApiExceptionFilter(Serilog.ILogger logger)
        {
            Logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is BusinessRuleValidationException businessException)
            {
                HandleBusinessRuleException(context, businessException);
            }
            else if (context.Exception is ValidationException  validationException)
            {
                HandleFluentValidationException(context, validationException);
            }
            else if(context.Exception is UnauthorizedAccessException unauthorizedAccessException)
            {
                HandleUnauthorizedAccessException(context, unauthorizedAccessException);
            }
            else
            {
                HandleUnknownException(context, context.Exception);
            }

            
            Logger.Error(context.Exception, "An exception occurred during request processing.");

            context.ExceptionHandled = true;
        }
        private void HandleBusinessRuleException(ExceptionContext context, BusinessRuleValidationException exception)
        {
            var details = new ValidationProblemDetails()
            {
                Type = "https.tools.ietf.org/html/rfc7231#section-6.5.8", 
                Title = "A business rule conflict occurred.",
                Status = StatusCodes.Status409Conflict,
                Detail = exception.Message
            };

            string fieldName = "General"; 
            if (exception is DuplicateFieldException exName) fieldName = exName.FieldName;

            details.Errors.Add(fieldName, new[] { exception.Message });
            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status409Conflict
            };
        }
        private void HandleUnauthorizedAccessException(ExceptionContext context, UnauthorizedAccessException exception)
        {
            var details = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Access denied: " + exception.Message,
                Type = "https.tools.ietf.org/html/rfc7231#section-6.5.3"
            };
            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
        private void HandleFluentValidationException(ExceptionContext context, ValidationException exception)
        {
            var details = new ValidationProblemDetails(
                exception.Errors.GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray()))
            {
                Type = "https.tools.ietf.org/html/rfc7231#section-6.5.1", 
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest
            };

            context.Result = new BadRequestObjectResult(details);
        }

        private void HandleUnknownException(ExceptionContext context, Exception exception)
        {
            var details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred: " + exception.Message,
                Type = "https.tools.ietf.org/html/rfc7231#section-6.6.1"
            };

            context.Result = new ObjectResult(details)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
