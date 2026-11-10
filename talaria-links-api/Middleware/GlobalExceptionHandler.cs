using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using talaria_links_api.Common.Exceptions;

namespace talaria_links_api.Middleware
{
    public class GlobalExceptionHandler(
        IProblemDetailsService problemDetails,
        ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, title) = exception switch
            {
                AppException app => (app.StatusCode, app.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            if (status >= 500)
                logger.LogError(exception, "Unhandled exception");

            httpContext.Response.StatusCode = status;
            return await problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = { Status = status, Title = title }
            });
        }
    }
}
