using FlashSales.Application.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlashSales.Infrastructure.Http
{
    public static class ClientCredentialsExtensions
    {
        public static IServiceCollection AddClientCredentials(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ClientCredentialsOptions>(configuration.GetSection(ClientCredentialsOptions.SectionName));
            services.AddHttpClient(HttpClientNames.ClientCredentialsToken);

            return services;
        }

        public static IHttpClientBuilder AddClientCredentialsHandler(this IHttpClientBuilder builder, string scope)
        {
            return builder.AddHttpMessageHandler(sp => new ClientCredentialsDelegatingHandler(
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<IOptions<ClientCredentialsOptions>>(),
                sp.GetRequiredService<ICacheService>(),
                sp.GetRequiredService<ILogger<ClientCredentialsDelegatingHandler>>(),
                scope));
        }
    }
}