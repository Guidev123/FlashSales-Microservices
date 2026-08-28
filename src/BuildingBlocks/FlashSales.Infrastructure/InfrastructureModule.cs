using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using FlashSales.Application.Abstractions;
using FlashSales.Application.Behaviors;
using FlashSales.Application.Bus;
using FlashSales.Application.Cache;
using FlashSales.Application.Messaging;
using FlashSales.Application.Storage;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure.Authentication;
using FlashSales.Infrastructure.Authorization;
using FlashSales.Infrastructure.Bus;
using FlashSales.Infrastructure.Cache;
using FlashSales.Infrastructure.Factories;
using FlashSales.Infrastructure.Interceptors;
using FlashSales.Infrastructure.Middlewares;
using FlashSales.Infrastructure.Observability;
using FlashSales.Infrastructure.Storage;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MidR.DependencyInjection;
using Serilog;
using StackExchange.Redis;
using System.Reflection;

namespace FlashSales.Infrastructure
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructureModule(this IServiceCollection services, IConfiguration configuration, IEnumerable<Assembly> assemblies)
        {
            services
                .AddApplication([.. assemblies])
                .AddCache(configuration)
                .AddBlobStorage(configuration)
                .AddConnectionFactory(configuration)
                .AddAuthenticationExtensions()
                .AddAuthorizationExtensions()
                .AddServiceBus(configuration)
                .AddExceptionHandler()
                .AddObservabilityHealthChecks(configuration);

            services.AddOpenApi();

            services.AddTransient<CallerLoggingMiddleware>();
            services.AddTransient<AccountActivationMiddleware>();

            return services;
        }

        private static IServiceCollection AddApplication(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

            var assembliesArray = assemblies.ToArray();

            services
                .AddMidR(args: assembliesArray).WithBehaviors(cfg =>
                    {
                        cfg.AddBehavior(typeof(RequestLoggingBehavior<,>)).WithPriority(1);
                        cfg.AddBehavior(typeof(RequestValidationBehavior<,>)).WithPriority(2);
                        cfg.AddBehavior(typeof(RequestTransactionBehavior<,>)).WithPriority(3);
                        cfg.AddBehavior(typeof(NotificationLoggingBehavior<>)).WithPriority(1);
                        cfg.AddBehavior(typeof(OutboxIdempotencyBehavior<>)).WithPriority(2);
                        cfg.AddBehavior(typeof(InboxIdempotencyBehavior<>)).WithPriority(3);
                    });
            services.AddSingleton(TimeProvider.System);
            services.AddScoped<IDomainEventCollector, DomainEventCollector>();
            services.AddScoped<DomainEventsInterceptor>();

            return services;
        }

        public static IServiceCollection AddServiceBus(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<ServiceBusOptions>(configuration.GetSection(ServiceBusOptions.SectionName));

            var section = configuration.GetSection("ServiceBus");
            var fullyQualifiedNamespace = section["FullyQualifiedNamespace"];
            var connectionString = section["ConnectionString"];

            var clientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp,
                RetryOptions = new ServiceBusRetryOptions
                {
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromMilliseconds(800),
                    MaxDelay = TimeSpan.FromSeconds(60)
                }
            };

            services.AddSingleton(_ =>
            {
                if (!string.IsNullOrWhiteSpace(fullyQualifiedNamespace))
                    return new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential(), clientOptions);

                if (!string.IsNullOrWhiteSpace(connectionString))
                    return new ServiceBusClient(connectionString, clientOptions);

                throw new InvalidOperationException(
                    "Configure 'ServiceBus:FullyQualifiedNamespace' (Managed Identity) " +
                    "or 'ServiceBus:ConnectionString' (dev local) on appsettings.json.");
            });

            services.AddSingleton<IEventBus, AzureServiceBus>();

            return services;
        }

        private static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!);
                services.TryAddSingleton(connectionMultiplexer);

                services.AddStackExchangeRedisCache(options =>
                {
                    options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer);
                });

                services.TryAddSingleton<ICacheService, CacheService>();
            }
            catch
            {
                services.TryAddSingleton<ICacheService, CacheService>();
                services.AddDistributedMemoryCache();
            }

            return services;
        }

        private static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.SectionName));

            services.AddSingleton<IBlobStorageService, BlobStorageService>();
            services.AddSingleton((sp) =>
            {
                var blobOptions = sp.GetRequiredService<IOptions<BlobStorageOptions>>();

                return new BlobServiceClient(blobOptions.Value.ConnectionString);
            });

            return services;
        }

        private static IServiceCollection AddConnectionFactory(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

            services.AddSingleton(new SqlConnectionFactory(connectionString));

            return services;
        }

        private static IServiceCollection AddAuthenticationExtensions(this IServiceCollection services)
        {
            return services.AddAuthenticationInternal();
        }

        private static IServiceCollection AddAuthorizationExtensions(this IServiceCollection services)
        {
            return services.AddAuthorizationInternal();
        }

        private static IServiceCollection AddExceptionHandler(this IServiceCollection services)
        {
            services.AddExceptionHandler<GlobalExceptionMiddleware>();
            services.AddProblemDetails();

            return services;
        }

        public static WebApplication UseInfrastructureModule(this WebApplication app)
        {
            app.UseExceptionHandler();

            app.MapEndpoints();

            if (!app.Environment.IsEnvironment("Testing"))
            {
                app.MapOpenApi().AllowAnonymous();

                app.MapHealthChecks("/health/live", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains(HealthChecksExtensions.LiveTag),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                }).AllowAnonymous();

                app.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains(HealthChecksExtensions.ReadyTag),
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                }).AllowAnonymous();

                app.UseSerilogRequestLogging();
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<CallerLoggingMiddleware>();
            app.UseMiddleware<AccountActivationMiddleware>();

            return app;
        }
    }
}