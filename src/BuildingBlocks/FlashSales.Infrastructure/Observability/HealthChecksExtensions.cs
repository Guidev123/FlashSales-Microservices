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

        public static IServiceCollection AddCoreHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient(nameof(OidcMetadataHealthCheck));
            services.Configure<OidcMetadataHealthCheckOptions>(options =>
                options.MetadataAddress = configuration[OidcMetadataHealthCheckOptions.SectionName]);

            services
                .AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), [LiveTag])
                .AddCheck<OidcMetadataHealthCheck>("oidc-metadata", tags: [ReadyTag]);

            return services;
        }

        public static IServiceCollection AddCacheHealthCheck(this IServiceCollection services)
        {
            services
                .AddHealthChecks()
                .AddRedis(sp => sp.GetRequiredService<IConnectionMultiplexer>(), name: "redis", tags: [ReadyTag]);

            return services;
        }

        public static IServiceCollection AddServiceBusHealthCheck(this IServiceCollection services)
        {
            services
                .AddHealthChecks()
                .AddCheck<ServiceBusClientHealthCheck>("service-bus", tags: [ReadyTag]);

            return services;
        }

        public static IServiceCollection AddPostgresHealthCheck(this IServiceCollection services, string connectionString)
        {
            services
                .AddHealthChecks()
                .AddNpgSql(connectionString, name: "postgres", tags: [ReadyTag]);

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