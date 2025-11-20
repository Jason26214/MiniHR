using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MiniHR.Domain.Exceptions;

namespace MiniHR.WebAPI.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
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
            // 1. Log the error
            _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            // 2. Core refactoring: Use switch expression to determine "Status" and "Title" based on exception type
            var (statusCode, title, detail) = exception switch
            {
                // If it's a "duplicate email exception", return 409 Conflict
                DuplicateEmailException => (StatusCodes.Status409Conflict, "Conflict", exception.Message),

                // In the future, if there's a "employee not found exception", add a line here
                // NotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),

                // Default case: treat all as 500 Internal Server Error
                _ => (StatusCodes.Status500InternalServerError, "Server Error", "An internal server error occurred.")
            };

            // 3. Build standard RFC 7807 response body
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path // Tell the frontend which endpoint caused the error
            };

            // 4. Send the response
            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}