using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Observability;
using Modules.Coupons.Infrastructure;
using Serilog;

const string ServiceName = "Coupons";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddObservabilityLogging(ServiceName);

    builder.Services
        .AddCoreInfrastructure(builder.Configuration, CouponsModule.Assemblies)
        .AddObservabilityTracing(builder.Configuration, ServiceName)
        .AddCouponsModule(builder.Configuration);

    builder
        .Build()
        .UseInfrastructureModule()
        .Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}