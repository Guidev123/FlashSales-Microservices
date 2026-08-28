using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Observability;
using Modules.Orders.Infrastructure;

const string ServiceName = "Orders";

var builder = WebApplication.CreateBuilder(args);

builder.AddObservabilityLogging(ServiceName);

builder.Services
    .AddInfrastructureModule(builder.Configuration, OrdersModule.Assemblies)
    .AddObservabilityTracing(builder.Configuration, ServiceName)
    .AddOrdersModule(builder.Configuration);

builder
    .Build()
    .UseInfrastructureModule()
    .Run();