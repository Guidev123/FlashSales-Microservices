using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using Modules.Catalog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCoreInfrastructure(builder.Configuration, CatalogModule.Assemblies)
    .AddCatalogModule(builder.Configuration);

var app = builder.Build();

app.MapEndpoints();

app.Run();

public partial class Program;
