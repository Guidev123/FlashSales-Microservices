using FlashSales.Application.Abstractions;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Extensions;
using FlashSales.Infrastructure.Interceptors;
using FlashSales.Infrastructure.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Coupons.Application.Coupons.Services;
using Modules.Coupons.Domain.Coupons.Repositories;
using Modules.Coupons.Endpoints;
using Modules.Coupons.Infrastructure.Database;
using Modules.Coupons.Infrastructure.Database.Repositories;
using System.Reflection;

namespace Modules.Coupons.Infrastructure
{
    public static class CouponsModule
    {
        public static readonly Assembly[] Assemblies =
        [
            Application.AssemblyReference.Assembly,
            Domain.AssemblyReference.Assembly,
            Assembly.GetExecutingAssembly()
        ];

        public static IServiceCollection AddCouponsModule(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddData(configuration)
                .AddOutbox(configuration)
                .AddInbox(configuration)
                .AddCache(configuration)
                .AddCacheHealthCheck()
                .AddServiceBus(configuration)
                .AddModulePermissions(Schemas.Coupons)
                .AddServiceBusHealthCheck()
                .AddEndpoints();
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

            services.AddDbContext<CouponsDbContext>((sp, cfg) =>
            {
                cfg.UseNpgsql(connectionString, npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Coupons);
                });

                cfg.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

            services.AddScoped<ICouponQueryService, CouponQueryService>();
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddModuleUnitOfWork<UnitOfWork>();

            return services;
        }

        private static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.AddEndpoints(typeof(EndpointsModule).Assembly);
            return services;
        }

        private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleInbox<IUnitOfWork>(
                configuration,
                "Coupons",
                Schemas.Coupons,
                Users.Contracts.IntegrationEvents.Topics.RoleAssigned,
                Users.Contracts.IntegrationEvents.Topics.RoleUnassigned,
                Users.Contracts.IntegrationEvents.Topics.RolePermissionGranted,
                Users.Contracts.IntegrationEvents.Topics.RolePermissionRevoked);

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleOutbox<IUnitOfWork>(configuration, "Coupons", Schemas.Coupons);

            return services;
        }
    }
}