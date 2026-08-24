using FlashSales.Application.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlashSales.Infrastructure.Http
{
    public static class OnBehalfOfExtensions
    {
        public static IServiceCollection AddOnBehalfOf(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddClientCredentials(configuration);
        }

        public static IHttpClientBuilder AddOnBehalfOfHandler(this IHttpClientBuilder builder, string audience, string? scope = null)
        {
            return builder.AddHttpMessageHandler(sp => new OnBehalfOfDelegatingHandler(
                sp.GetRequiredService<IHttpContextAccessor>(),
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<IOptions<ClientCredentialsOptions>>(),
                sp.GetRequiredService<ICacheService>(),
                sp.GetRequiredService<ILogger<OnBehalfOfDelegatingHandler>>(),
                audience,
                scope));
        }
    }
}
