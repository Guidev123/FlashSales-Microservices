using FlashSales.Domain.DomainObjects;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FlashSales.Infrastructure.Middlewares
{
    internal sealed class GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Unhandled exception occurred");

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
                Title = "Server failure",
                Detail = GetExceptionMessage(exception)
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static string GetExceptionMessage(Exception? exception)
        {
            return exception switch
            {
                FlashSalesException flashSalesException when flashSalesException.Error?.Description is not null => flashSalesException.Error.Description,
                _ => "Unknown error"
            };
        }
    }
}