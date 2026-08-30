using FlashSales.Application.Abstractions;
using FlashSales.Application.Authorization;
using FlashSales.Application.Bus;
using FlashSales.Application.Inbox;
using FlashSales.Application.Outbox;
using FlashSales.Infrastructure.Authorization;
using FlashSales.Infrastructure.Database;
using FlashSales.Infrastructure.Inbox;
using FlashSales.Infrastructure.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlashSales.Infrastructure.Extensions
{
    public static class ModuleServiceExtensions
    {
        public static IServiceCollection AddModuleUnitOfWork<TUnitOfWorkImpl>(
            this IServiceCollection services)
            where TUnitOfWorkImpl : class, IUnitOfWork
        {
            services.AddScoped<TUnitOfWorkImpl>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TUnitOfWorkImpl>());
            return services;
        }

        public static IServiceCollection AddModuleOutbox<TUnitOfWork>(
            this IServiceCollection services,
            IConfiguration configuration,
            string moduleName,
            string schema)
            where TUnitOfWork : class, IUnitOfWork
        {
            services.AddScoped<ModuleOutboxRepository<TUnitOfWork>>(sp =>
                new ModuleOutboxRepository<TUnitOfWork>(sp.GetRequiredService<TUnitOfWork>(), schema));

            services.AddScoped<IOutboxRepository>(sp =>
                sp.GetRequiredService<ModuleOutboxRepository<TUnitOfWork>>());

            services.Configure<OutboxOptions>(moduleName,
                configuration.GetSection(OutboxOptions.SectionName));

            services.AddSingleton<ModuleOutboxProcessor<TUnitOfWork>>(sp =>
                new ModuleOutboxProcessor<TUnitOfWork>(
                    sp.GetRequiredService<ILogger<ModuleOutboxProcessor<TUnitOfWork>>>(),
                    sp.GetRequiredService<IOptionsMonitor<OutboxOptions>>(),
                    sp,
                    moduleName));

            services.AddSingleton<IOutboxBatchProcessor>(
                sp => sp.GetRequiredService<ModuleOutboxProcessor<TUnitOfWork>>());

            services.AddHostedService(
                sp => sp.GetRequiredService<ModuleOutboxProcessor<TUnitOfWork>>());

            return services;
        }

        public static IServiceCollection AddModuleInbox<TUnitOfWork>(
            this IServiceCollection services,
            IConfiguration configuration,
            string moduleName,
            string schema,
            params string[] topics)
            where TUnitOfWork : class, IUnitOfWork
        {
            services.AddScoped<ModuleInboxRepository<TUnitOfWork>>(sp =>
                new ModuleInboxRepository<TUnitOfWork>(sp.GetRequiredService<TUnitOfWork>(), schema));

            services.AddScoped<IInboxRepository>(sp =>
                sp.GetRequiredService<ModuleInboxRepository<TUnitOfWork>>());

            services.Configure<InboxOptions>(moduleName,
                configuration.GetSection(InboxOptions.SectionName));

            services.AddSingleton<ModuleInboxProcessor<TUnitOfWork>>(sp =>
                new ModuleInboxProcessor<TUnitOfWork>(
                    sp.GetRequiredService<ILogger<ModuleInboxProcessor<TUnitOfWork>>>(),
                    sp.GetRequiredService<IOptionsMonitor<InboxOptions>>(),
                    sp,
                    moduleName));

            services.AddSingleton<IInboxBatchProcessor>(
                sp => sp.GetRequiredService<ModuleInboxProcessor<TUnitOfWork>>());

            services.AddHostedService(
                sp => sp.GetRequiredService<ModuleInboxProcessor<TUnitOfWork>>());

            var subscriptionName = $"{moduleName.ToLower()}.sub";

            services.AddSingleton<ModuleInboxConsumer<TUnitOfWork>>(sp =>
                new ModuleInboxConsumer<TUnitOfWork>(
                    sp.GetRequiredService<IEventBus>(),
                    sp,
                    sp.GetRequiredService<ILogger<ModuleInboxConsumer<TUnitOfWork>>>(),
                    moduleName,
                    subscriptionName,
                    topics));

            services.AddHostedService(
                sp => sp.GetRequiredService<ModuleInboxConsumer<TUnitOfWork>>());

            return services;
        }

        public static IServiceCollection AddModulePermissions(
            this IServiceCollection services,
            string schema)
        {
            services.AddScoped<IPermissionRepository>(sp =>
                new PermissionRepository(sp.GetRequiredService<IUnitOfWork>(), schema));

            services.AddTransient<IPermissionService>(sp =>
                new PermissionService(sp.GetRequiredService<IUnitOfWork>(), schema));

            return services;
        }
    }
}