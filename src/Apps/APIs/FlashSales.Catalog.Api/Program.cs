using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Observability;
using Modules.Catalog.Infrastructure;
using Serilog;

const string ServiceName = "Catalog";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddObservabilityLogging(ServiceName);

    builder.Services
        .AddCoreInfrastructure(builder.Configuration, CatalogModule.Assemblies)
        .AddObservabilityTracing(builder.Configuration, ServiceName)
        .AddCatalogModule(builder.Configuration);

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