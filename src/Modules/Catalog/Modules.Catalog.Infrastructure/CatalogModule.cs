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
using Modules.Catalog.Application;
using Modules.Catalog.Application.Products.Services;
using Modules.Catalog.Contracts;
using Modules.Catalog.Domain.Products.Repositories;
using Modules.Catalog.Domain.Sellers.Repositories;
using Modules.Catalog.Endpoints;
using Modules.Catalog.Infrastructure.Authorization;
using Modules.Catalog.Infrastructure.Database;
using Modules.Catalog.Infrastructure.Database.Repositories;
using Modules.Catalog.Infrastructure.Options;
using Modules.Catalog.Infrastructure.PublicApi;
using System.Reflection;

namespace Modules.Catalog.Infrastructure
{
    public static class CatalogModule
    {
        public static readonly Assembly[] Assemblies = [
            Application.AssemblyReference.Assembly,
            Domain.AssemblyReference.Assembly,
            Contracts.AssemblyReference.Assembly,
            Assembly.GetExecutingAssembly(),
            Users.Contracts.AssemblyReference.Assembly,
        ];

        public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddData(configuration)
                .AddCache(configuration)
                .AddCacheHealthCheck()
                .AddBlobStorage(configuration)
                .AddBlobStorageHealthCheck()
                .AddServiceBus(configuration)
                .AddServiceBusHealthCheck()
                .AddOutbox(configuration)
                .AddInbox(configuration)
                .AddEndpoints()
                .AddPublicApi()
                .AddServices()
                .AddGrpcServices(configuration);

            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

            services.AddDbContext<CatalogDbContext>((sp, cfg) =>
            {
                cfg.UseNpgsql(connectionString, npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Catalog);
                });
                cfg.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

            services.AddPostgresHealthCheck(connectionString);

            services.AddModuleUnitOfWork<UnitOfWork>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ISellerRepository, SellerRepository>();
            services.AddScoped<IProductQueryService, ProductQueryService>();

            return services;
        }

        private static IServiceCollection AddOutbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleOutbox<IUnitOfWork>(configuration, "Catalog", Schemas.Catalog);
            return services;
        }

        private static IServiceCollection AddInbox(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddModuleInbox<IUnitOfWork>(
                configuration, "Catalog", Schemas.Catalog,
                Users.Contracts.IntegrationEvents.Topics.SellerActivated,
                Users.Contracts.IntegrationEvents.Topics.SellerProfilePictureUpdated,
                Users.Contracts.IntegrationEvents.Topics.UserProfileUpdated);
            return services;
        }

        private static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            services.AddEndpoints(typeof(EndpointsModule).Assembly);
            return services;
        }

        private static IServiceCollection AddPublicApi(this IServiceCollection services)
        {
            services.AddTransient<ICatalogPublicApi, CatalogPublicApi>();
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