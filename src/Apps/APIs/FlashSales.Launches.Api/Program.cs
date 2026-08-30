using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Observability;
using Modules.Launches.Infrastructure;
using Serilog;

const string ServiceName = "Launches";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddObservabilityLogging(ServiceName);

    builder.Services
        .AddCoreInfrastructure(builder.Configuration, LaunchesModule.Assemblies)
        .AddObservabilityTracing(builder.Configuration, ServiceName)
        .AddLaunchesModule(builder.Configuration);

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