using FlashSales.Infrastructure;
using Modules.Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureModule(builder.Configuration, OrdersModule.Assemblies)
    .AddOrdersModule(builder.Configuration);

builder
    .Build()
    .UseInfrastructureModule()
    .Run();