using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using Modules.Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddInfrastructureModule(builder.Configuration, UsersModule.Assemblies)
    .AddUsersModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
