using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using Modules.Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureModule(builder.Configuration, UsersModule.Assemblies)
    .AddUsersModule(builder.Configuration);

var app = builder.Build();

app.MapEndpoints();

app.Run();

public partial class Program;
