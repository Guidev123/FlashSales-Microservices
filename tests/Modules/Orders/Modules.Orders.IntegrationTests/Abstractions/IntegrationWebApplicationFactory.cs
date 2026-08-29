using Azure.Messaging.ServiceBus;
using DotNet.Testcontainers.Builders;
using FlashSales.Application.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.Orders.Infrastructure.Database;
using Npgsql;
using System.Net;
using Testcontainers.PostgreSql;
using Testcontainers.ServiceBus;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Modules.Orders.IntegrationTests.Abstractions
{
    public class IntegrationWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private static readonly Guid TokenMappingGuid = Guid.Parse("11111111-0000-0000-0000-000000000001");
        private static readonly Guid ReserveMappingGuid = Guid.Parse("11111111-0000-0000-0000-000000000002");
        private static readonly Guid ReleaseMappingGuid = Guid.Parse("11111111-0000-0000-0000-000000000003");
        private static readonly Guid CheckoutMappingGuid = Guid.Parse("11111111-0000-0000-0000-000000000004");

        private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("flashsales_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        private readonly ServiceBusContainer _serviceBusContainer = new ServiceBusBuilder("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
            .WithAcceptLicenseAgreement(true)
            .WithResourceMapping(
                new FileInfo(Path.Combine(AppContext.BaseDirectory, "Abstractions", "servicebus.config.json")),
                new FileInfo("/ServiceBus_Emulator/ConfigFiles/Config.json"))
            .Build();

        private readonly WireMockServer _wireMock = WireMockServer.Start();

        internal FakePermissionService PermissionService { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(AppContext.BaseDirectory);

            builder.UseSetting("ConnectionStrings:Postgres", _postgresContainer.GetConnectionString());
            builder.UseSetting("Authentication:MetadataAddress",
                "https://test.auth/.well-known/openid-configuration");
            builder.UseSetting("Authentication:TokenValidationParameters:ValidIssuer",
                "https://test.auth/realms/flash-sales-dev");
            builder.UseSetting("Users:KeyCloak:AdminUrl", "https://test.keycloak/admin/realms/");
            builder.UseSetting("Users:KeyCloak:BaseUrl", "https://test.keycloak/realms/");
            builder.UseSetting("Users:KeyCloak:CurrentRealm", "flash-sales-dev");
            builder.UseSetting("Users:KeyCloak:ConfidentialClientId", "test-client");
            builder.UseSetting("Users:KeyCloak:ConfidentialClientSecret", "test-secret");

            builder.UseSetting("ClientCredentials:Authority", _wireMock.Url);
            builder.UseSetting("ClientCredentials:ClientId", "flash-sales-orders-svc");
            builder.UseSetting("ClientCredentials:ClientSecret", "test-secret");

            builder.UseSetting("ApiOptions:LaunchesApi:BaseUrl", _wireMock.Url);
            builder.UseSetting("ApiOptions:LaunchesApi:Scope", "launches.stock.write");
            builder.UseSetting("ApiOptions:PaymentsApi:BaseUrl", _wireMock.Url);
            builder.UseSetting("ApiOptions:PaymentsApi:Scope", "");
            builder.UseSetting("ApiOptions:PaymentsApi:Audience", "flash-sales-payments");

            builder.UseSetting("ApiOptions:UsersApi:Scope", "users.permissions.read");

            builder.ConfigureAppConfiguration(cfg =>
                cfg.AddJsonFile(
                    Path.Combine(AppContext.BaseDirectory, "modules.orders.Testing.json"),
                    optional: true));

            builder.ConfigureServices(services =>
            {
                RemoveHostedServices(services);
                ReplaceServiceBusClient(services);
                ReplacePermissionService(services);
                ReplaceHttpContextAccessor(services);
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            return base.CreateHost(builder);
        }

        public async Task InitializeAsync()
        {
            SeedWireMockDefaults();

            await _serviceBusContainer.StartAsync();
            await _postgresContainer.StartAsync();
            await MigrateAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgresContainer.DisposeAsync();
            await _serviceBusContainer.DisposeAsync();
            _wireMock.Stop();
        }

        public async Task ResetDatabaseAsync()
        {
            PermissionService.Reset();
            SeedWireMockDefaults();

            await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand("""
                DELETE FROM orders."Orders";
                DELETE FROM orders."OrderCreationSagas";
                DELETE FROM orders."Launches";
                DELETE FROM orders."OutboxMessageConsumers";
                DELETE FROM orders."OutboxMessages";
                DELETE FROM orders."InboxMessageConsumers";
                DELETE FROM orders."InboxMessages";
                """, connection);

            await cmd.ExecuteNonQueryAsync();
        }

        public string GetConnectionString() => _postgresContainer.GetConnectionString();

        internal void SeedWireMockDefaults()
        {
            _wireMock.ResetMappings();
            StubTokenEndpoint();
            StubLaunchesReserveSuccess();
            StubLaunchesReleaseSuccess();
            StubPaymentsCheckoutSuccess();
        }

        internal void StubTokenEndpoint()
        {
            _wireMock
                .Given(Request.Create().WithPath("/protocol/openid-connect/token").UsingPost())
                .WithGuid(TokenMappingGuid)
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new { access_token = "test-access-token", expires_in = 3600 }));
        }

        internal void StubLaunchesReserveSuccess()
        {
            _wireMock
                .Given(Request.Create().WithPath("/api/v1/launches/stock/reserve").UsingPost())
                .WithGuid(ReserveMappingGuid)
                .RespondWith(Response.Create().WithStatusCode(200));
        }

        internal void StubLaunchesReserveFailure(HttpStatusCode statusCode = HttpStatusCode.Conflict, string body = "Insufficient stock")
        {
            _wireMock
                .Given(Request.Create().WithPath("/api/v1/launches/stock/reserve").UsingPost())
                .WithGuid(ReserveMappingGuid)
                .RespondWith(Response.Create().WithStatusCode((int)statusCode).WithBody(body));
        }

        internal void StubLaunchesReleaseSuccess()
        {
            _wireMock
                .Given(Request.Create().WithPath("/api/v1/launches/stock/release").UsingPost())
                .WithGuid(ReleaseMappingGuid)
                .RespondWith(Response.Create().WithStatusCode(200));
        }

        internal void StubPaymentsCheckoutSuccess()
        {
            _wireMock
                .Given(Request.Create().WithPath("/api/v1/payments/checkout").UsingPost())
                .WithGuid(CheckoutMappingGuid)
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithBodyAsJson(new
                    {
                        PaymentId = Guid.NewGuid(),
                        AttemptId = Guid.NewGuid(),
                        CheckoutUrl = $"https://checkout.test/{Guid.NewGuid():N}"
                    }));
        }

        internal void StubPaymentsCheckoutFailure(HttpStatusCode statusCode = HttpStatusCode.BadRequest, string body = "Gateway unavailable")
        {
            _wireMock
                .Given(Request.Create().WithPath("/api/v1/payments/checkout").UsingPost())
                .WithGuid(CheckoutMappingGuid)
                .RespondWith(Response.Create().WithStatusCode((int)statusCode).WithBody(body));
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

        private void ReplacePermissionService(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IPermissionService));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton<IPermissionService>(PermissionService);
        }

        private void ReplaceHttpContextAccessor(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IHttpContextAccessor));
            if (descriptor is not null)
                services.Remove(descriptor);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = "Bearer test-subject-token";

            services.AddSingleton<IHttpContextAccessor>(new FixedHttpContextAccessor(httpContext));
        }

        private sealed class FixedHttpContextAccessor(HttpContext httpContext) : IHttpContextAccessor
        {
            public HttpContext? HttpContext
            {
                get => httpContext;
                set { }
            }
        }

        private async Task MigrateAsync()
        {
            using var scope = Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<OrdersDbContext>().Database.MigrateAsync();
        }
    }
}
