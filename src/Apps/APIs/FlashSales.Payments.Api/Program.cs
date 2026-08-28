using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Observability;
using Modules.Payments.Infrastructure;

const string ServiceName = "Payments";

var builder = WebApplication.CreateBuilder(args);

builder.AddObservabilityLogging(ServiceName);

builder.Services
    .AddInfrastructureModule(builder.Configuration, PaymentsModule.Assemblies)
    .AddObservabilityTracing(builder.Configuration, ServiceName)
    .AddPaymentsModule(builder.Configuration);

builder
    .Build()
    .UseInfrastructureModule()
    .Run();