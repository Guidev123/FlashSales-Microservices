using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;

namespace FlashSales.Infrastructure.Observability
{
    public static class TracingExtensions
    {
        public static IServiceCollection AddObservabilityTracing(
            this IServiceCollection services,
            IConfiguration configuration,
            string serviceName,
            Action<TracerProviderBuilder>? configureAdditionalInstrumentation = null)
        {
            services
                .AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.Filter = httpContext =>
                                !httpContext.Request.Path.StartsWithSegments("/health");
                        })
                        .AddHttpClientInstrumentation()
                        .AddGrpcClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation(options =>
                        {
                            options.SetDbStatementForText = true;
                        });

                    var connectionMultiplexer = services
                        .FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IConnectionMultiplexer))
                        ?.ImplementationInstance as IConnectionMultiplexer;

                    if (connectionMultiplexer is not null)
                    {
                        tracing.AddRedisInstrumentation(connectionMultiplexer);
                    }

                    configureAdditionalInstrumentation?.Invoke(tracing);

                    var otlpEndpoint = configuration["OpenTelemetry:Otlp:Endpoint"];
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    {
                        tracing.AddOtlpExporter(otlp =>
                        {
                            otlp.Endpoint = new Uri(otlpEndpoint);
                            otlp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                        });
                    }
                    else
                    {
                        tracing.AddConsoleExporter();
                    }
                });

            return services;
        }
    }
}
