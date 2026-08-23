using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using Modules.Catalog.Infrastructure;
using Modules.Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureModule(builder.Configuration,
    [
        ..CatalogModule.Assemblies,
        ..UsersModule.Assemblies
    ])
    .AddCatalogModule(builder.Configuration)
    .AddUsersModule(builder.Configuration);

var app = builder.Build();

app.MapEndpoints();

app.Run();

public partial class Program;
