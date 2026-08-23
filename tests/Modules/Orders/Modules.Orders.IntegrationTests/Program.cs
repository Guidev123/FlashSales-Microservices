using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using Modules.Launches.Infrastructure;
using Modules.Orders.Infrastructure;
using Modules.Payments.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureModule(builder.Configuration,
    [
        ..OrdersModule.Assemblies,
        ..LaunchesModule.Assemblies,
        ..PaymentsModule.Assemblies
    ])
    .AddOrdersModule(builder.Configuration)
    .AddLaunchesModule(builder.Configuration)
    .AddPaymentsModule(builder.Configuration);

var app = builder.Build();

app.MapEndpoints();

app.Run();

public partial class Program;
