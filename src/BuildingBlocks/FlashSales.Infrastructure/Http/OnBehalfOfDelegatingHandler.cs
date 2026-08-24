using FlashSales.Application.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace FlashSales.Infrastructure.Http
{
    internal sealed class OnBehalfOfDelegatingHandler(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        IOptions<ClientCredentialsOptions> options,
        ICacheService cache,
        ILogger<OnBehalfOfDelegatingHandler> logger,
        string audience,
        string? scope
        ) : DelegatingHandler
    {
        private const string GrantType = "urn:ietf:params:oauth:grant-type:token-exchange";
        private const string SubjectTokenType = "urn:ietf:params:oauth:token-type:access_token";

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        private readonly ClientCredentialsOptions _options = options.Value;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var subjectToken = GetSubjectToken();

            try
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetExchangedTokenAsync(subjectToken, cancellationToken));

                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode != HttpStatusCode.Unauthorized)
                    return response;

                await cache.RemoveAsync(CacheKey(subjectToken), cancellationToken);
                response.Dispose();

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetExchangedTokenAsync(subjectToken, cancellationToken));

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Token OBO re-exchanged for audience: {Audience}", audience);
                }

                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to exchange OBO token: {Audience} - {ClientId} - {RequestUri}",
                    audience, _options.ClientId, request.RequestUri);

                throw;
            }
        }

        private string GetSubjectToken()
        {
            var authorizationHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(authorizationHeader) || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "On-behalf-of flow requires an inbound request carrying a Bearer token to exchange — none was found on the current HttpContext.");

            return authorizationHeader["Bearer ".Length..];
        }

        private string CacheKey(string subjectToken)
        {
            var subjectHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(subjectToken)));
            return $"on-behalf-of:{_options.ClientId}:{audience}:{scope}:{subjectHash}";
        }

        private async Task<string> GetExchangedTokenAsync(string subjectToken, CancellationToken cancellationToken)
        {
            var cacheKey = CacheKey(subjectToken);

            var cached = await cache.GetAsync<CachedToken>(cacheKey, cancellationToken);
            if (cached is not null)
                return cached.AccessToken;

            var semaphore = _locks.GetOrAdd(cacheKey, static _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                cached = await cache.GetAsync<CachedToken>(cacheKey, cancellationToken);
                if (cached is not null)
                    return cached.AccessToken;

                var token = await ExchangeTokenAsync(subjectToken, cancellationToken);

                var ttl = TimeSpan.FromSeconds(Math.Max(token.ExpiresIn - 60, 30));
                await cache.SetAsync(cacheKey, new CachedToken(token.AccessToken), ttl, cancellationToken);

                return token.AccessToken;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task<TokenResponse> ExchangeTokenAsync(string subjectToken, CancellationToken cancellationToken)
        {
            var tokenClient = httpClientFactory.CreateClient(HttpClientNames.ClientCredentialsToken);

            var parameters = new Dictionary<string, string>
            {
                ["grant_type"] = GrantType,
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
                ["subject_token"] = subjectToken,
                ["subject_token_type"] = SubjectTokenType,
                ["audience"] = audience
            };

            if (!string.IsNullOrWhiteSpace(scope))
                parameters["scope"] = scope;

            using var content = new FormUrlEncodedContent(parameters);

            using var response = await tokenClient.PostAsync(
                $"{_options.Authority}/protocol/openid-connect/token",
                content,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
                ?? throw new InvalidOperationException("Keycloak did not return a token-exchange response.");
        }

        private sealed record CachedToken(string AccessToken);

        private sealed record TokenResponse(
            [property: JsonPropertyName("access_token")] string AccessToken,
            [property: JsonPropertyName("expires_in")] int ExpiresIn);
    }
}
