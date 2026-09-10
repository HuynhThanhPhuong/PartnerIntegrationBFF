using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace API
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            int statusCode;
            string title;

            if (exception is ValidationException)
            {
                statusCode = StatusCodes.Status400BadRequest;
                title = "Validation failed";
            }
            else
            {
                statusCode = StatusCodes.Status500InternalServerError;
                title = "An unexpected error occurred";
            }

            _logger.LogError(exception, "Request failed with {StatusCode}", statusCode);

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                title,
                status = statusCode,
                detail = exception.Message
            }, cancellationToken);

            return true;
        }
    }
}
