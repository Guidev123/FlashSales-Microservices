using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using Modules.Coupons.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCoreInfrastructure(builder.Configuration, CouponsModule.Assemblies)
    .AddCouponsModule(builder.Configuration);

var app = builder.Build();

app.MapEndpoints();

app.Run();

public partial class Program;
