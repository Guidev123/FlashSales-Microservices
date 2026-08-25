using FlashSales.Application.Abstractions;
using FlashSales.Application.Authorization;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure.Extensions;
using FlashSales.Infrastructure.Interceptors;
using FlashSales.Users.Contracts.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Payments.Application.Payments;
using Modules.Payments.Application.Payments.Services;
using Modules.Payments.Contracts;
using Modules.Payments.Domain.Payments.Repositories;
using Modules.Payments.Endpoints;
using Modules.Payments.Infrastructure.Authorization;
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
                .AddOutbox(configuration)
                .AddInbox(configuration)
                .AddEndpoints()
                .AddGateway(configuration)
                .AddJobs(configuration)
                .AddServices();

            return services;
        }

        private static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PaymentsDbContext>((sp, cfg) =>
            {
                cfg.UseNpgsql(configuration.GetConnectionString("Postgres"), npgSqlCfg =>
                {
                    npgSqlCfg.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Payments);
                });
                cfg.AddInterceptors(sp.GetRequiredService<DomainEventsInterceptor>());
            });

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
                configuration, "Payments", Schemas.Payments);
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
            services.AddTransient<IPermissionService, PermissionService>();

            return services;
        }

        private static IServiceCollection AddGrpcServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddGrpcClient<UserPermissionsService.UserPermissionsServiceClient>(options =>
            {
                options.Address = new Uri(configuration["ExternalServices:UsersApi"]!);
            }).AddResilienceHandler(nameof(HttpResiliencePipelineExtensions), pipeline => pipeline.ConfigureGrpcResilience());

            return services;
        }
    }
}