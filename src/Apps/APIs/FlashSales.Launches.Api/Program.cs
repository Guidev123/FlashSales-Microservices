using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Observability;
using Modules.Launches.Infrastructure;

const string ServiceName = "Launches";

var builder = WebApplication.CreateBuilder(args);

builder.AddObservabilityLogging(ServiceName);

builder.Services
    .AddCoreInfrastructure(builder.Configuration, LaunchesModule.Assemblies)
    .AddObservabilityTracing(builder.Configuration, ServiceName)
    .AddLaunchesModule(builder.Configuration);

builder
    .Build()
    .UseInfrastructureModule()
    .Run();