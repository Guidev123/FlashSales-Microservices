using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FlashSales.Infrastructure.KeyVault
{
    public static class KeyVaultExtensions
    {
        public static WebApplicationBuilder AddKeyVaultSecrets(this WebApplicationBuilder builder)
        {
            if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
                return builder;

            var vaultUri = builder.Configuration.GetConnectionString("KeyVault");

            if (string.IsNullOrWhiteSpace(vaultUri))
            {
                if (builder.Environment.IsProduction())
                    throw new InvalidOperationException("Connection string 'KeyVault' (vault URI) is mandatory in Production.");

                return builder;
            }

            builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());

            return builder;
        }
    }
}