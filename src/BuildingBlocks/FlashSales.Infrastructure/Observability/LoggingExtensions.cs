using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;

namespace FlashSales.Infrastructure.Observability
{
    public static class LoggingExtensions
    {
        public static WebApplicationBuilder AddObservabilityLogging(this WebApplicationBuilder builder, string serviceName)
        {
            if (builder.Environment.IsEnvironment("Testing"))
                return builder;

            builder.Host.UseSerilog((context, loggerConfig) =>
            {
                loggerConfig
                    .ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithSpan()
                    .Enrich.WithProperty("Service", serviceName);
            });

            return builder;
        }
    }
}
