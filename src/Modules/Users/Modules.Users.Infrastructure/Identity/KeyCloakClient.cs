using Microsoft.Extensions.Options;
using Modules.Users.Application.Users.Dtos;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Modules.Users.Infrastructure.Identity
{
    internal sealed class KeyCloakClient(HttpClient httpClient, IOptions<KeyCloakOptions> options)
    {
        private readonly KeyCloakOptions _options = options.Value;

        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy =
            HttpPolicyExtensions.HandleTransientHttpError()
                .WaitAndRetryAsync(options.Value.DeleteMaxRetryAttempts,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        internal async Task<string> RegisterAsync(UserRepresentationDto user, CancellationToken cancellationToken = default)
        {
            var httpResponseMessage = await httpClient.PostAsJsonAsync($"{_options.CurrentRealm}/users", user, cancellationToken);

            httpResponseMessage.EnsureSuccessStatusCode();

            return ExtractIdentityIdFromLocationHeader(httpResponseMessage);
        }

        internal async Task SetUserAttributesAsync(string identityProviderId, Dictionary<string, List<string>> attributes, CancellationToken cancellationToken = default)
        {
            var payload = new
            {
                email = attributes.GetValueOrDefault("email")?[0],
                attributes
            };

            var httpResponseMessage = await httpClient.PutAsJsonAsync(
                $"{_options.CurrentRealm}/users/{identityProviderId}",
                payload,
                cancellationToken);

            httpResponseMessage.EnsureSuccessStatusCode();
        }

        internal async Task AssignRoleAsync(string identityProviderId, string roleName, CancellationToken cancellationToken = default)
        {
            var roleResponse = await httpClient.GetAsync(
                $"{_options.CurrentRealm}/roles/{roleName}",
                cancellationToken);

            roleResponse.EnsureSuccessStatusCode();

            var role = await roleResponse.Content.ReadFromJsonAsync<RoleRepresentationDto>(cancellationToken: cancellationToken);

            var httpResponseMessage = await httpClient.PostAsJsonAsync(
                $"{_options.CurrentRealm}/users/{identityProviderId}/role-mappings/realm",
                new[] { role },
                cancellationToken);

            httpResponseMessage.EnsureSuccessStatusCode();
        }

        internal async Task DeleteAsync(string identityProviderId, CancellationToken cancellationToken = default)
        {
            var httpResponseMessage = await _retryPolicy.ExecuteAsync(() => httpClient.DeleteAsync(
                $"{_options.CurrentRealm}/users/{identityProviderId}",
                cancellationToken));

            httpResponseMessage.EnsureSuccessStatusCode();
        }

        private static string ExtractIdentityIdFromLocationHeader(HttpResponseMessage httpResponseMessage)
        {
            const string USER_SEGMENT_NAME = "users/";

            var locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;
            if (string.IsNullOrEmpty(locationHeader))
                throw new InvalidOperationException("Location Header is null");

            var userSegmentValueIndex = locationHeader.IndexOf(USER_SEGMENT_NAME, StringComparison.InvariantCultureIgnoreCase);

            return locationHeader[(userSegmentValueIndex + USER_SEGMENT_NAME.Length)..];
        }
    }

    internal sealed record RoleRepresentationDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name);
}