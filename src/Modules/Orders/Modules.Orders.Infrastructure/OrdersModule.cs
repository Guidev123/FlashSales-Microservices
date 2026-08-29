using FlashSales.Application.Abstractions;
using FlashSales.Application.Authorization;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Extensions;
using FlashSales.Infrastructure.Http;
using FlashSales.Infrastructure.Interceptors;
using FlashSales.Infrastructure.Observability;
using FlashSales.Users.Contracts.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Launches.Contracts;
using Modules.Orders.Application.Orders.Sagas;
using Modules.Orders.Application.Orders.Services;
using Modules.Orders.Domain.Launches.Repositories;
using Modules.Orders.Domain.Orders.Repositories;
using Modules.Orders.Endpoints;
using Modules.Orders.Infrastructure.Authorization;
using Modules.Orders.Infrastructure.Database;
using Modules.Orders.Infrastructure.Database.Repositories;
using Modules.Orders.Infrastructure.Jobs;
using Modules.Orders.Infrastructure.Options;
using Modules.Orders.Infrastructure.Services;
using Modules.Payments.Contracts;
using System.Reflection;

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
    }
}