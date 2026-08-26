using FlashSales.Infrastructure;
using Modules.Payments.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureModule(builder.Configuration, PaymentsModule.Assemblies)
    .AddPaymentsModule(builder.Configuration);

builder
    .Build()
    .UseInfrastructureModule()
    .Run();