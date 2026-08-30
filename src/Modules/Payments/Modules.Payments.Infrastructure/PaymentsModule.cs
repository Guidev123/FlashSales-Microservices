using FlashSales.Application.Abstractions;
using FlashSales.Application.Authorization;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Extensions;
using FlashSales.Infrastructure.Http;
using FlashSales.Infrastructure.Interceptors;
using FlashSales.Infrastructure.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Payments.Application.Payments;
using Modules.Payments.Application.Payments.Services;
using Modules.Payments.Contracts;
using Modules.Payments.Domain.Payments.Repositories;
using Modules.Payments.Endpoints;
using Modules.Payments.Infrastructure.Database;
using Modules.Payments.Infrastructure.Database.Repositories;
using Modules.Payments.Infrastructure.Gateway;
using Modules.Payments.Infrastructure.Jobs;
using System.Reflection;

namespace Modules.Payments.Infrastructure
{
    public static class PaymentsModule
    {
        public static readonly Assembly[] Assemblies =
        [
            Application.AssemblyReference.Assembly,
            Domain.AssemblyReference.Assembly,
            AssemblyReference.Assembly,
            Assembly.GetExecutingAssembly(),
        ];

        public static IServiceCollection AddPaymentsModule(this IServiceCollection services, IConfiguration configuration)
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
                .AddGateway(configuration)
                .AddJobs(configuration)
                .AddServices()
                .AddModulePermissions(Schemas.Payments);

            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

            services.AddDbContext<PaymentsDbContext>((sp, cfg) =>
            {
                cfg.UseNpgsql(connectionString, npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Payments);
                });
                cfg.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

            services.AddPostgresHealthCheck(connectionString);

            services.AddModuleUnitOfWork<UnitOfWork>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleOutbox<IUnitOfWork>(configuration, "Payments", Schemas.Payments);
            return services;
        }

        private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleInbox<IUnitOfWork>(
                configuration, "Payments", Schemas.Payments,
                Users.Contracts.IntegrationEvents.Topics.RoleAssigned,
                Users.Contracts.IntegrationEvents.Topics.RoleUnassigned,
                Users.Contracts.IntegrationEvents.Topics.RolePermissionGranted,
                Users.Contracts.IntegrationEvents.Topics.RolePermissionRevoked);
            return services;
        }

        private static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.AddEndpoints(typeof(EndpointsModule).Assembly);
            return services;
        }

        private static IServiceCollection AddGateway(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
            services.AddTransient<IPaymentGatewayService, StripePaymentGatewayService>();
            return services;
        }

        private static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PaymentsJobsOptions>(configuration.GetSection(PaymentsJobsOptions.SectionName));
            services.AddHostedService<PaymentReconciliationJob>();
            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<PaymentOutcomeProcessor>();

            return services;
        }
    }
}