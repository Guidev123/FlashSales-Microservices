using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using FlashSales.Infrastructure.Factories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.Catalog.Infrastructure.Database;
using Modules.Launches.Infrastructure.Database;
using Modules.Orders.Infrastructure.Database;
using Modules.Payments.Application.Payments.Services;
using Modules.Payments.Infrastructure.Database;
using Modules.Users.Infrastructure.Database;
using Npgsql;
using DotNet.Testcontainers.Builders;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;
using Testcontainers.ServiceBus;

namespace Modules.IntegrationTests.Abstractions
{
    public class IntegrationWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("flashsales_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(10000))
            .WithCommand("--skipApiVersionCheck")
            .Build();

        private readonly ServiceBusContainer _serviceBusContainer = new ServiceBusBuilder()
            .WithAcceptLicenseAgreement(true)
            .WithResourceMapping(
                new FileInfo(Path.Combine(AppContext.BaseDirectory, "Abstractions", "servicebus.config.json")),
                new FileInfo("/ServiceBus_Emulator/ConfigFiles/Config.json"))
            .Build();

        internal FakePaymentGatewayService PaymentGateway { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Postgres", _postgresContainer.GetConnectionString());
            builder.UseSetting("BlobStorage:ConnectionString", _azuriteContainer.GetConnectionString());
            builder.UseSetting("Authentication:MetadataAddress",
                "https://test.auth/.well-known/openid-configuration");
            builder.UseSetting("Authentication:TokenValidationParameters:ValidIssuer",
                "https://test.auth/realms/flash-sales-dev");
            builder.UseSetting("Users:KeyCloak:AdminUrl", "https://test.keycloak/admin/realms/");
            builder.UseSetting("Users:KeyCloak:BaseUrl", "https://test.keycloak/realms/");
            builder.UseSetting("Users:KeyCloak:CurrentRealm", "flash-sales-dev");
            builder.UseSetting("Users:KeyCloak:ConfidentialClientId", "test-client");
            builder.UseSetting("Users:KeyCloak:ConfidentialClientSecret", "test-secret");

            builder.ConfigureServices(services =>
            {
                RemoveHostedServices(services);
                ReplaceServiceBusClient(services);
                ReplaceSqlConnectionFactory(services);
                ReplaceBlobServiceClient(services);
                ReplacePaymentGatewayService(services);
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            return base.CreateHost(builder);
        }

        public async Task InitializeAsync()
        {
            await _serviceBusContainer.StartAsync();

            await Task.WhenAll(
                _postgresContainer.StartAsync(),
                _azuriteContainer.StartAsync());

            await MigrateAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgresContainer.DisposeAsync();
            await _azuriteContainer.DisposeAsync();
            await _serviceBusContainer.DisposeAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            PaymentGateway.Reset();

            await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand("""
                DELETE FROM catalog."ProductImages";
                DELETE FROM catalog."Products";
                DELETE FROM catalog."Sellers";
                DELETE FROM catalog."Categories";
                DELETE FROM catalog."OutboxMessageConsumers";
                DELETE FROM catalog."OutboxMessages";
                DELETE FROM catalog."InboxMessageConsumers";
                DELETE FROM catalog."InboxMessages";
                DELETE FROM users."OutboxMessageConsumers";
                DELETE FROM users."OutboxMessages";
                DELETE FROM users."InboxMessageConsumers";
                DELETE FROM users."InboxMessages";
                DELETE FROM orders."Orders";
                DELETE FROM orders."OrderCreationSagas";
                DELETE FROM orders."Launches";
                DELETE FROM orders."OutboxMessageConsumers";
                DELETE FROM orders."OutboxMessages";
                DELETE FROM orders."InboxMessageConsumers";
                DELETE FROM orders."InboxMessages";
                DELETE FROM payments."PaymentAttempts";
                DELETE FROM payments."Payments";
                DELETE FROM payments."OutboxMessageConsumers";
                DELETE FROM payments."OutboxMessages";
                DELETE FROM payments."InboxMessageConsumers";
                DELETE FROM payments."InboxMessages";
                DELETE FROM launches."StockReservations";
                DELETE FROM launches."Launches";
                DELETE FROM launches."Sellers";
                DELETE FROM launches."OutboxMessageConsumers";
                DELETE FROM launches."OutboxMessages";
                DELETE FROM launches."InboxMessageConsumers";
                DELETE FROM launches."InboxMessages";
                """, connection);

            await cmd.ExecuteNonQueryAsync();
        }

        public string GetConnectionString() => _postgresContainer.GetConnectionString();

        public Task SimulateDbFailureAsync() => _postgresContainer.StopAsync();

        public async Task RestoreDbAsync()
        {
            await _postgresContainer.StartAsync();
            Npgsql.NpgsqlConnection.ClearAllPools();
        }

        private static void RemoveHostedServices(IServiceCollection services)
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);
        }

        private void ReplaceServiceBusClient(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ServiceBusClient));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton(new ServiceBusClient(_serviceBusContainer.GetConnectionString()));
        }

        private void ReplaceSqlConnectionFactory(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(SqlConnectionFactory));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton(new SqlConnectionFactory(_postgresContainer.GetConnectionString()));
        }

        private void ReplaceBlobServiceClient(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(BlobServiceClient));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton(new BlobServiceClient(_azuriteContainer.GetConnectionString()));
        }

        private void ReplacePaymentGatewayService(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPaymentGatewayService));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton<IPaymentGatewayService>(PaymentGateway);
        }

        private async Task MigrateAsync()
        {
            using var scope = Services.CreateScope();
            var sp = scope.ServiceProvider;

            await sp.GetRequiredService<UsersDbContext>().Database.MigrateAsync();
            await sp.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();
            await sp.GetRequiredService<LaunchesDbContext>().Database.MigrateAsync();
            await sp.GetRequiredService<PaymentsDbContext>().Database.MigrateAsync();
            await sp.GetRequiredService<OrdersDbContext>().Database.MigrateAsync();
        }
    }
}