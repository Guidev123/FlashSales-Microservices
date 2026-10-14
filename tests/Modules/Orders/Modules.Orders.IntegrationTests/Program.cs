using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using FlashSales.Infrastructure.FeatureFlags;
using Modules.Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddFeatureFlags();

builder.Services
    .AddCoreInfrastructure(builder.Configuration, OrdersModule.Assemblies)
    .AddOrdersModule(builder.Configuration);

var app = builder.Build();

app.MapEndpoints();

app.Run();

public partial class Program;
