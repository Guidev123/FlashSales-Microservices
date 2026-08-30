using FlashSales.Application.Abstractions;
using FlashSales.Application.Authorization;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Extensions;
using FlashSales.Infrastructure.Http;
using FlashSales.Infrastructure.Interceptors;
using FlashSales.Infrastructure.Mongo;
using FlashSales.Infrastructure.Observability;
using FlashSales.Users.Contracts.Protos;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Launches.Contracts;
using Modules.Orders.Application.Orders.Sagas;
using Modules.Orders.Application.Orders.Services;
using Modules.Orders.Domain.Launches.Repositories;
using Modules.Orders.Domain.Orders.DomainEvents;
using Modules.Orders.Domain.Orders.Entities;
using Modules.Orders.Domain.Orders.Repositories;
using Modules.Orders.Endpoints;
using Modules.Orders.Infrastructure.Authorization;
using Modules.Orders.Infrastructure.Database;
using Modules.Orders.Infrastructure.Database.EventSourcing;
using Modules.Orders.Infrastructure.Database.Repositories;
using Modules.Orders.Infrastructure.Jobs;
using Modules.Orders.Infrastructure.Options;
using Modules.Orders.Infrastructure.Services;
using Modules.Payments.Contracts;
using MongoDB.Driver;
using System.Reflection;
using Weasel.Core;

namespace Modules.Orders.Infrastructure
{
    public static class OrdersModule
    {
        public static readonly Assembly[] Assemblies =
        [
            typeof(OrdersModule).Assembly,
            typeof(Application.AssemblyReference).Assembly,
        ];

        public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddData(configuration)
                .AddCache(configuration)
                .AddCacheHealthCheck()
                .AddServiceBus(configuration)
                .AddServiceBusHealthCheck()
                .AddOutbox(configuration)
                .AddInbox(configuration)
                .AddEndpoints()
                .AddJobs(configuration)
                .AddApiServices(configuration)
                .AddSagasOrchestrators()
                .AddServices()
                .AddEventSourcing(configuration)
                .AddGrpcServices(configuration);

            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

            services.AddDbContext<OrdersDbContext>((sp, cfg) =>
            {
                cfg.UseNpgsql(connectionString, npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Orders);
                });
                cfg.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

            services.AddPostgresHealthCheck(connectionString);

            services.AddModuleUnitOfWork<UnitOfWork>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ILaunchRepository, LaunchRepository>();
            services.AddScoped<IOrderCreationSagaRepository, OrderCreationSagaRepository>();
            services.AddScoped<IOrderQueryService, OrderQueryService>();

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleOutbox<IUnitOfWork>(configuration, "Orders", Schemas.Orders);
            return services;
        }

        private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleInbox<IUnitOfWork>(
                configuration, "Orders", Schemas.Orders,
                Launches.Contracts.IntegrationEvents.Topics.LaunchActivated,
                Launches.Contracts.IntegrationEvents.Topics.LaunchEnded,
                Launches.Contracts.IntegrationEvents.Topics.LaunchCancelled,
                Payments.Contracts.IntegrationEvents.Topics.PaymentCompleted,
                Payments.Contracts.IntegrationEvents.Topics.PaymentFailed,
                Payments.Contracts.IntegrationEvents.Topics.PaymentRefunded);
            return services;
        }

        private static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.AddEndpoints(typeof(EndpointsModule).Assembly);
            return services;
        }

        private static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<OrdersJobsOptions>(configuration.GetSection(OrdersJobsOptions.SectionName));
            services.AddHostedService<OrderExpirySweepJob>();
            services.AddHostedService<OrderSagaSweepJob>();
            return services;
        }

        private static IServiceCollection AddSagasOrchestrators(this IServiceCollection services)
        {
            services.AddScoped<OrderCreationSagaOrchestrator>();

            return services;
        }

        private static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection(ApiOptions.SectionName).Get<ApiOptions>()
                ?? throw new InvalidOperationException($"Configuration section '{ApiOptions.SectionName}' is missing.");

            services.AddCustomHttpClientWithClientCredentialsAuth<ILaunchesPublicApi, LaunchesApiService>(configuration, options.LaunchesApi);
            services.AddCustomHttpClientWithOnBehalfOfAuth<IPaymentsPublicApi, PaymentsApiService>(configuration, options.PaymentsApi);

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IPermissionService, PermissionService>();
            return services;
        }

        private static IServiceCollection AddGrpcServices(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection(ApiOptions.SectionName).Get<ApiOptions>()
                ?? throw new InvalidOperationException($"Configuration section '{ApiOptions.SectionName}' is missing.");

            services.AddCustomGrpcClientWithClientCredentialsAuth<UserPermissionsService.UserPermissionsServiceClient>(configuration, options.UsersApi);
            services.AddGrpcServiceHealthCheck("users-grpc", options.UsersApi.BaseUrl);

            return services;
        }

        private static IServiceCollection AddEventSourcing(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

            services.AddScoped<OutboxDomainEventListener>();

            services
                .AddMongo(configuration)
                .AddMongoHealthCheck();

            var mongoConnectionString = configuration.GetConnectionString("Mongo")
                ?? throw new InvalidOperationException("Connection string 'Mongo' is not configured.");
            var mongoOptions = configuration.GetSection(MongoOptions.SectionName).Get<MongoOptions>()
                ?? throw new InvalidOperationException($"Configuration section '{MongoOptions.SectionName}' is missing.");
            var mongoDatabase = new MongoClient(mongoConnectionString).GetDatabase(mongoOptions.DatabaseName);

            services.AddMarten(cfg =>
            {
                cfg.DatabaseSchemaName = Schemas.OrdersEventSourcing;
                cfg.Connection(connectionString);
                cfg.ApplyDomainConfiguration();

                cfg.Events.Subscribe(new MongoOrderProjectionSubscription(mongoDatabase), o =>
                {
                    o.Name = "MongoOrderProjection";
                    o.FilterIncomingEventsOnStreamType(typeof(Order));
                    o.IncludeType<OrderCreatedDomainEvent>();
                    o.IncludeType<OrderPaymentProcessingStartedDomainEvent>();
                    o.IncludeType<OrderConfirmedDomainEvent>();
                    o.IncludeType<OrderCancelledDomainEvent>();
                    o.IncludeType<OrderRefundedDomainEvent>();
                });
            })
            .AddAsyncDaemon(DaemonMode.HotCold)
            .ApplyAllDatabaseChangesOnStartup();

            return services;
        }

        private static void ApplyDomainConfiguration(this StoreOptions options)
        {
            options.UseSystemTextJsonForSerialization(
                enumStorage: EnumStorage.AsString,
                configure: settings => settings.Converters.Add(new OrderJsonConverter()));

            options.Events.AddEventType<OrderCreatedDomainEvent>();
            options.Events.AddEventType<OrderConfirmedDomainEvent>();
            options.Events.AddEventType<OrderPaymentProcessingStartedDomainEvent>();
            options.Events.AddEventType<OrderRefundedDomainEvent>();
            options.Events.AddEventType<OrderCancelledDomainEvent>();

            options.Projections.Snapshot<Order>(SnapshotLifecycle.Inline);

            options.Schema.For<Order>().Index(x => new { x.CustomerId, x.LaunchId }, x =>
            {
                x.IsUnique = true;
                x.Predicate = "(data ->> 'Status') IN ('AwaitingPayment', 'PaymentProcessing')";
            });

            options.Schema.For<Order>().Index(x => new { x.Status, x.ExpiresAt });
        }
    }
}