using Azure.Storage.Blobs;
using FlashSales.Infrastructure.Observability.HealthChecks;
using Grpc.Health.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace FlashSales.Infrastructure.Observability
{
    public static class HealthChecksExtensions
    {
        public const string LiveTag = "live";
        public const string ReadyTag = "ready";

        public static IServiceCollection AddObservabilityHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient(nameof(OidcMetadataHealthCheck));
            services.Configure<OidcMetadataHealthCheckOptions>(options =>
                options.MetadataAddress = configuration[OidcMetadataHealthCheckOptions.SectionName]);

            var postgresConnectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

            services
                .AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), [LiveTag])
                .AddNpgSql(postgresConnectionString, name: "postgres", tags: [ReadyTag])
                .AddRedis(sp => sp.GetRequiredService<IConnectionMultiplexer>(), name: "redis", tags: [ReadyTag])
                .AddCheck<ServiceBusClientHealthCheck>("service-bus", tags: [ReadyTag])
                .AddCheck<OidcMetadataHealthCheck>("oidc-metadata", tags: [ReadyTag]);

            return services;
        }

        public static IServiceCollection AddBlobStorageHealthCheck(this IServiceCollection services)
        {
            services
                .AddHealthChecks()
                .AddAzureBlobStorage(
                    sp => sp.GetRequiredService<BlobServiceClient>(),
                    name: "blob-storage",
                    tags: [ReadyTag]);

            return services;
        }

        public static IServiceCollection AddGrpcServiceHealthCheck(this IServiceCollection services, string name, string baseUrl)
        {
            services.AddGrpcClient<Health.HealthClient>(client => client.Address = new Uri(baseUrl));

            services
                .AddHealthChecks()
                .AddCheck<GrpcServiceHealthCheck>(name, tags: [ReadyTag]);

            return services;
        }
    }
}