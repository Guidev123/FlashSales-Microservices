using FlashSales.Infrastructure;
using FlashSales.Infrastructure.Observability;
using Modules.Users.Infrastructure;
using Serilog;

const string ServiceName = "Users";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddObservabilityLogging(ServiceName);

    builder.Services
        .AddCoreInfrastructure(builder.Configuration, UsersModule.Assemblies)
        .AddObservabilityTracing(builder.Configuration, ServiceName)
        .AddUsersModule(builder.Configuration);

    var app = builder.Build();
    app.UseInfrastructureModule()
        .MapGrpcEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}