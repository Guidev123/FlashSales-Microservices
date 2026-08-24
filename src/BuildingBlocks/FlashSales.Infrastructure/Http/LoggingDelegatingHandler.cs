using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FlashSales.Infrastructure.Http
{
    public sealed class LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid();

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("HTTP Request Started: {RequestId} - {Method} - {RequestUri}", requestId, request.Method, request.RequestUri);
            }

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                stopwatch.Stop();

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("HTTP Request Completed: {RequestId} - {Method} - {RequestUri} - {StatusCode} - {ElapsedMilliseconds}ms",
                        requestId, request.Method, request.RequestUri, response.StatusCode, stopwatch.ElapsedMilliseconds);
                }

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                logger.LogError(ex, "HTTP Request Failed: {RequestId} - {Method} - {RequestUri} - {ElapsedMilliseconds}ms",
                    requestId, request.Method, request.RequestUri, stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}