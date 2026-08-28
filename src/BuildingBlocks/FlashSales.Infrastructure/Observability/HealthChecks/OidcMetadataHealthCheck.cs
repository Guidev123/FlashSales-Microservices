using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FlashSales.Infrastructure.Observability.HealthChecks
{
    internal sealed class OidcMetadataHealthCheck(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<OidcMetadataHealthCheckOptions> options
        ) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var metadataAddress = options.CurrentValue.MetadataAddress;
            if (string.IsNullOrWhiteSpace(metadataAddress))
            {
                return HealthCheckResult.Unhealthy("Authentication:MetadataAddress is not configured.");
            }

            using var client = httpClientFactory.CreateClient(nameof(OidcMetadataHealthCheck));

            using var response = await client.GetAsync(metadataAddress, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"OIDC metadata endpoint returned {(int)response.StatusCode}.");
        }
    }

    internal sealed class OidcMetadataHealthCheckOptions
    {
        public const string SectionName = "Authentication:MetadataAddress";
        public string? MetadataAddress { get; set; }
    }
}