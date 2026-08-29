using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using Modules.Payments.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCoreInfrastructure(builder.Configuration, PaymentsModule.Assemblies)
    .AddPaymentsModule(builder.Configuration);

var app = builder.Build();

app.MapEndpoints();

app.Run();

public partial class Program;
