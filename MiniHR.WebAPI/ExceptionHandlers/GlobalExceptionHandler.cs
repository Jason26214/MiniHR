using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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
            {
                // error log to internal
                _logger.LogError(
                    exception,
                    "An unhandled exception occurred: {Message}",
                    exception.Message);

                // from here to end. send error log to frontend
                // RFC 7807 International Standard Defined "Error Response Format"
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server Error",
                    Detail = "An internal server error occurred. Please contact the administrator."
                };

                // response to frontend
                httpContext.Response.StatusCode = problemDetails.Status.Value;

                await httpContext.Response
                    .WriteAsJsonAsync(problemDetails, cancellationToken);

                return true;
            }
        }
    }
}
