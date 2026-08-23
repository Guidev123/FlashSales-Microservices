using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using Modules.Launches.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddInfrastructureModule(builder.Configuration, LaunchesModule.Assemblies)
    .AddLaunchesModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
