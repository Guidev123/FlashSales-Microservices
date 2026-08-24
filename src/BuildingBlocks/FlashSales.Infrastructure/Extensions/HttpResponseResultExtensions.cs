using FlashSales.Domain.Results;
using FlashSales.Infrastructure.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;

namespace FlashSales.Infrastructure.Extensions
{
    public static class HttpResponseResultExtensions
    {
        public static async Task<Result<TValue>> ToResultAsync<TValue>(
            this Task<HttpResponseMessage> responseTask,
            ILogger logger,
            Func<HttpStatusCode, string, Error>? mapError = null,
            CancellationToken ct = default)
        {
            HttpResponseMessage response;

            try
            {
                response = await responseTask;
            }
            catch (HttpTimeoutException ex)
            {
                logger.LogWarning(ex, "Request timed out: {Message}", ex.Message);
                return Result.Failure<TValue>(
                    Error.Problem("Http.Timeout", ex.Message));
            }
            catch (HttpTransportException ex)
            {
                logger.LogError(ex, "Transport failure: {Message}", ex.Message);
                return Result.Failure<TValue>(
                    Error.Problem("Http.TransportFailure", ex.Message));
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                var error = (mapError ?? DefaultErrorMapper)(response.StatusCode, body);
                return Result.Failure<TValue>(error);
            }

            var value = await response.Content.ReadFromJsonAsync<TValue>(cancellationToken: ct);

            return value is not null
                ? Result.Success(value)
                : Result.Failure<TValue>(Error.NullValue);
        }

        public static async Task<Result> ToResultAsync(
            this Task<HttpResponseMessage> responseTask,
            ILogger logger,
            Func<HttpStatusCode, string, Error>? mapError = null,
            CancellationToken ct = default)
        {
            HttpResponseMessage response;

            try
            {
                response = await responseTask;
            }
            catch (HttpTimeoutException ex)
            {
                logger.LogWarning(ex, "Request timed out: {Message}", ex.Message);
                return Result.Failure(Error.Problem("Http.Timeout", ex.Message));
            }
            catch (HttpTransportException ex)
            {
                logger.LogError(ex, "Transport failure: {Message}", ex.Message);
                return Result.Failure(Error.Problem("Http.TransportFailure", ex.Message));
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                var error = (mapError ?? DefaultErrorMapper)(response.StatusCode, body);
                return Result.Failure(error);
            }

            return Result.Success();
        }

        private static Error DefaultErrorMapper(HttpStatusCode statusCode, string body) => statusCode switch
        {
            HttpStatusCode.NotFound =>
                Error.NotFound("Http.NotFound", string.IsNullOrWhiteSpace(body) ? "Resource not found" : body),

            HttpStatusCode.Conflict =>
                Error.Conflict("Http.Conflict", body),

            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity =>
                Error.Invalid("Http.InvalidRequest", body),

            _ =>
                Error.Problem("Http.UnexpectedError", $"Status {(int)statusCode}: {body}")
        };
    }
}