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
using Modules.Launches.Application.Launches.Services;
using Modules.Launches.Domain.Launches.Repositories;
using Modules.Launches.Domain.Sellers.Repositories;
using Modules.Launches.Endpoints;
using Modules.Launches.Infrastructure.Database;
using Modules.Launches.Infrastructure.Database.Repositories;
using Modules.Launches.Infrastructure.Jobs;
using System.Reflection;

namespace Modules.Launches.Infrastructure
{
    public static class LaunchesModule
    {
        public static readonly Assembly[] Assemblies =
        [
            Application.AssemblyReference.Assembly,
            Modules.Launches.Domain.AssemblyReference.Assembly,
            Contracts.AssemblyReference.Assembly,
            Assembly.GetExecutingAssembly(),
            Users.Contracts.AssemblyReference.Assembly,
        ];

        public static IServiceCollection AddLaunchesModule(this IServiceCollection services, IConfiguration configuration)
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
                .AddModulePermissions(Schemas.Launches);

            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

            services.AddDbContext<LaunchesDbContext>((sp, cfg) =>
            {
                cfg.UseNpgsql(connectionString, npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Launches);
                });
                cfg.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

            services.AddPostgresHealthCheck(connectionString);

            services.AddModuleUnitOfWork<UnitOfWork>();
            services.AddScoped<ILaunchRepository, LaunchRepository>();
            services.AddScoped<ISellerRepository, SellerRepository>();
            services.AddScoped<ILaunchQueryService, LaunchQueryService>();

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleOutbox<IUnitOfWork>(configuration, "Launches", Schemas.Launches);
            return services;
        }

        private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleInbox<IUnitOfWork>(
                configuration, "Launches", Schemas.Launches,
                Users.Contracts.IntegrationEvents.Topics.SellerActivated,
                Users.Contracts.IntegrationEvents.Topics.UserProfileUpdated,
                Users.Contracts.IntegrationEvents.Topics.SellerProfilePictureUpdated,
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

        private static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<LaunchesJobsOptions>(configuration.GetSection(LaunchesJobsOptions.SectionName));
            services.AddHostedService<LaunchActivatorJob>();
            services.AddHostedService<LaunchEnderJob>();
            return services;
        }

    }
}