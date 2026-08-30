using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Observability;
using Modules.Payments.Infrastructure;
using Serilog;

const string ServiceName = "Payments";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddObservabilityLogging(ServiceName);

    builder.Services
        .AddCoreInfrastructure(builder.Configuration, PaymentsModule.Assemblies)
        .AddObservabilityTracing(builder.Configuration, ServiceName)
        .AddPaymentsModule(builder.Configuration);

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