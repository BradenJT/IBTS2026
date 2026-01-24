using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IBTS2026.ApiService.Middleware
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title, detail) = MapException(exception);

            _logger.LogError(
                exception,
                "Exception occurred: {ExceptionType} - {Message}",
                exception.GetType().Name,
                exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            // Add validation errors if applicable
            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static (int StatusCode, string Title, string Detail) MapException(Exception exception)
        {
            return exception switch
            {
                ValidationException validationEx => (
                    StatusCodes.Status400BadRequest,
                    "Validation Failed",
                    "One or more validation errors occurred."),

                InvalidOperationException invalidOpEx => (
                    StatusCodes.Status409Conflict,
                    "Business Rule Violation",
                    invalidOpEx.Message),

                KeyNotFoundException notFoundEx => (
                    StatusCodes.Status404NotFound,
                    "Resource Not Found",
                    notFoundEx.Message),

                ArgumentException argEx => (
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    argEx.Message),

                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "You are not authorized to perform this action."),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred. Please try again later.")
            };
        }
    }
}
