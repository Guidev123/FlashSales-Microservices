using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Observability;
using Modules.Users.Infrastructure;

const string ServiceName = "Users";

var builder = WebApplication.CreateBuilder(args);

builder.AddObservabilityLogging(ServiceName);

builder.Services
    .AddCoreInfrastructure(builder.Configuration, UsersModule.Assemblies)
    .AddObservabilityTracing(builder.Configuration, ServiceName)
    .AddUsersModule(builder.Configuration);

var app = builder.Build();
app.UseInfrastructureModule()
    .MapGrpcEndpoints();

app.Run();