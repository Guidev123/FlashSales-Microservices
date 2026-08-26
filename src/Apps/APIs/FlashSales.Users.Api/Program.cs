using FlashSales.Infrastructure;
using Modules.Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureModule(builder.Configuration, UsersModule.Assemblies)
    .AddUsersModule(builder.Configuration);

var app = builder.Build();
app.UseInfrastructureModule()
    .MapGrpcEndpoints();

app.Run();