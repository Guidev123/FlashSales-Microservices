using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Observability;
using Modules.Catalog.Infrastructure;

const string ServiceName = "Catalog";

var builder = WebApplication.CreateBuilder(args);

builder.AddObservabilityLogging(ServiceName);

builder.Services
    .AddCoreInfrastructure(builder.Configuration, CatalogModule.Assemblies)
    .AddObservabilityTracing(builder.Configuration, ServiceName)
    .AddCatalogModule(builder.Configuration);

builder
    .Build()
    .UseInfrastructureModule()
    .Run();