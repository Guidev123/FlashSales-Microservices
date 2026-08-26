using FlashSales.Infrastructure;
using Modules.Catalog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureModule(builder.Configuration, CatalogModule.Assemblies)
    .AddCatalogModule(builder.Configuration);

builder
    .Build()
    .UseInfrastructureModule()
    .Run();