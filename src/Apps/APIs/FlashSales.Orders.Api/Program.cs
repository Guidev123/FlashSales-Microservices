using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using Modules.Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .AddInfrastructureModule(builder.Configuration, OrdersModule.Assemblies)
    .AddOrdersModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapEndpoints();

app.Run();
