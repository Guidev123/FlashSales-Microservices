using Azure.Identity;
using FlashSales.Application.FeatureFlags;
using FlashSales.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FeatureManagement;

namespace FlashSales.Infrastructure.FeatureFlags
{
    public static class FeatureFlagsExtensions
    {
        public static WebApplicationBuilder AddFeatureFlags(this WebApplicationBuilder builder)
        {
            var options = builder.Configuration.GetSection(FeatureFlagOptions.SectionName).Get<FeatureFlagOptions>()
                ?? new FeatureFlagOptions();

            var connectionString = builder.Configuration.GetConnectionString("AppConfiguration");

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var parsed = AzureConnectionString.Parse(connectionString);

                builder.Configuration.AddAzureAppConfiguration(appConfig =>
                {
                    if (parsed.HasCredentials("Secret"))
                        appConfig.Connect(connectionString);
                    else
                        appConfig.Connect(parsed.Endpoint, new DefaultAzureCredential());

                    appConfig.UseFeatureFlags(flags =>
                    {
                        if (!string.IsNullOrWhiteSpace(options.Label))
                            flags.Select(KeyFilter.Any, options.Label);

                        flags.SetRefreshInterval(TimeSpan.FromSeconds(options.RefreshIntervalInSeconds));
                    });
                });

                builder.Services.AddAzureAppConfiguration();
                builder.Services.AddSingleton<IStartupFilter, AzureAppConfigurationStartupFilter>();
            }

            builder.Services.AddHttpContextAccessor();
            builder.Services
                .AddFeatureManagement()
                .WithTargeting<ClaimsTargetingContextAccessor>();

            builder.Services.TryAddSingleton<IFeatureFlagService, FeatureFlagService>();

            return builder;
        }

        private sealed class AzureAppConfigurationStartupFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
                => app =>
                {
                    app.UseAzureAppConfiguration();
                    next(app);
                };
        }
    }
}