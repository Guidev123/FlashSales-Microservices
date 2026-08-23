using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using Modules.Launches.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureModule(builder.Configuration, LaunchesModule.Assemblies)
    .AddLaunchesModule(builder.Configuration);

var app = builder.Build();

app.MapEndpoints();

app.Run();

public partial class Program;
