using FlashSales.Infrastructure;
using Modules.Launches.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureModule(builder.Configuration, LaunchesModule.Assemblies)
    .AddLaunchesModule(builder.Configuration);

builder
    .Build()
    .UseInfrastructureModule()
    .Run();