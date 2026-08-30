using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Observability;
using Modules.Orders.Infrastructure;
using Serilog;

const string ServiceName = "Orders";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddObservabilityLogging(ServiceName);

    builder.Services
        .AddCoreInfrastructure(builder.Configuration, OrdersModule.Assemblies)
        .AddObservabilityTracing(builder.Configuration, ServiceName)
        .AddOrdersModule(builder.Configuration);

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