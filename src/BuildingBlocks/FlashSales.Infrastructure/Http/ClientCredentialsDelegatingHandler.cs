using FlashSales.Application.Cache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FlashSales.Infrastructure.Http
{
    internal sealed class ClientCredentialsDelegatingHandler(
        IHttpClientFactory httpClientFactory,
        IOptions<ClientCredentialsOptions> options,
        ICacheService cache,
        ILogger<ClientCredentialsDelegatingHandler> logger,
        string scope
        ) : DelegatingHandler
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        private readonly ClientCredentialsOptions _options = options.Value;

        private string CacheKey => $"client-credentials:{_options.ClientId}:{scope}";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync(cancellationToken));

                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode != HttpStatusCode.Unauthorized)
                    return response;

                await cache.RemoveAsync(CacheKey, cancellationToken);
                response.Dispose();

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync(cancellationToken));

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Token M2M generated for scope: {Scope}", scope);
                }

                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate M2M: {Scope} - {ClientId} - {RequestUri}",
                    scope, _options.ClientId, request.RequestUri);

                throw;
            }
        }

        private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            var cached = await cache.GetAsync<CachedToken>(CacheKey, cancellationToken);
            if (cached is not null)
                return cached.AccessToken;

            var semaphore = _locks.GetOrAdd(CacheKey, static _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                cached = await cache.GetAsync<CachedToken>(CacheKey, cancellationToken);
                if (cached is not null)
                    return cached.AccessToken;

                var token = await RequestTokenAsync(cancellationToken);

                var ttl = TimeSpan.FromSeconds(Math.Max(token.ExpiresIn - 60, 30));
                await cache.SetAsync(CacheKey, new CachedToken(token.AccessToken), ttl, cancellationToken);

                return token.AccessToken;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task<TokenResponse> RequestTokenAsync(CancellationToken cancellationToken)
        {
            var tokenClient = httpClientFactory.CreateClient(HttpClientNames.ClientCredentialsToken);

            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["scope"] = scope
            };

            using var content = new FormUrlEncodedContent(parameters);

            using var response = await tokenClient.PostAsync(
                $"{_options.Authority}/protocol/openid-connect/token",
                content,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
                ?? throw new InvalidOperationException("Keycloak did not return a token response.");
        }

        private sealed record CachedToken(string AccessToken);

        private sealed record TokenResponse(
            [property: JsonPropertyName("access_token")] string AccessToken,
            [property: JsonPropertyName("expires_in")] int ExpiresIn);
    }
}