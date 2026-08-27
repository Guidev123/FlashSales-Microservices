using Grpc.Net.ClientFactory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FlashSales.Infrastructure.Http
{
    public static class HttpClientExtensions
    {
        public static IServiceCollection AddCustomHttpClientWithClientCredentialsAuth<TInterface, TService>(
            this IServiceCollection services,
            IConfiguration configuration,
            HttpOptions options
            ) where TInterface : class
            where TService : class, TInterface
        {
            services.AddClientCredentials(configuration);
            services.AddSharedDelegatingHandlers();

            services.AddHttpClient<TInterface, TService>(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddHttpMessageHandler<LoggingDelegatingHandler>()
            .AddHttpMessageHandler<ExceptionTranslationDelegatingHandler>()
            .AddClientCredentialsHandler(options.Scope)
            .ConfigurePrimaryHttpMessageHandler(HttpMessageHandlerFactory.CreateSocketsHttpHandler)
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddResilienceHandler(nameof(ResiliencePipelineExtensions), (pipeline, context) =>
            {
                pipeline.ConfigureResilience(options);
            });

            return services;
        }

        public static IServiceCollection AddCustomHttpClientWithOnBehalfOfAuth<TInterface, TService>(
            this IServiceCollection services,
            IConfiguration configuration,
            HttpOptions options
            ) where TInterface : class
            where TService : class, TInterface
        {
            if (string.IsNullOrWhiteSpace(options.Audience))
                throw new InvalidOperationException($"'{nameof(HttpOptions.Audience)}' must be configured for an on-behalf-of HTTP client.");

            services.AddOnBehalfOf(configuration);
            services.AddSharedDelegatingHandlers();

            services.AddHttpClient<TInterface, TService>(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddHttpMessageHandler<LoggingDelegatingHandler>()
            .AddHttpMessageHandler<ExceptionTranslationDelegatingHandler>()
            .AddOnBehalfOfHandler(options.Audience, string.IsNullOrWhiteSpace(options.Scope) ? null : options.Scope)
            .ConfigurePrimaryHttpMessageHandler(HttpMessageHandlerFactory.CreateSocketsHttpHandler)
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddResilienceHandler(nameof(ResiliencePipelineExtensions), (pipeline, context) =>
            {
                pipeline.ConfigureResilience(options);
            });

            return services;
        }

        public static IServiceCollection AddCustomGrpcClientWithClientCredentialsAuth<TClient>(
            this IServiceCollection services,
            IConfiguration configuration,
            HttpOptions options
            ) where TClient : class
        {
            services.AddClientCredentials(configuration);
            services.AddSharedDelegatingHandlers();

            services.AddGrpcClient<TClient>(client =>
            {
                client.Address = new Uri(options.BaseUrl);
            })
            .AddHttpMessageHandler<LoggingDelegatingHandler>()
            .AddClientCredentialsHandler(options.Scope)
            .ConfigurePrimaryHttpMessageHandler(HttpMessageHandlerFactory.CreateSocketsHttpHandler)
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddResilienceHandler(nameof(ResiliencePipelineExtensions), (pipeline, context) =>
            {
                pipeline.ConfigureResilience(options);
            });

            return services;
        }

        private static IServiceCollection AddSharedDelegatingHandlers(this IServiceCollection services)
        {
            services.TryAddTransient<LoggingDelegatingHandler>();
            services.TryAddTransient<ExceptionTranslationDelegatingHandler>();
            return services;
        }
    }
}