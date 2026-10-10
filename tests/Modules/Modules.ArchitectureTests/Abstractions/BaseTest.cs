using System.Reflection;
using CatalogInfrastructure = Modules.Catalog.Infrastructure.CatalogModule;
using CouponsInfrastructure = Modules.Coupons.Infrastructure.CouponsModule;
using LaunchesInfrastructure = Modules.Launches.Infrastructure.LaunchesModule;
using OrdersInfrastructure = Modules.Orders.Infrastructure.OrdersModule;
using PaymentsInfrastructure = Modules.Payments.Infrastructure.PaymentsModule;
using UsersInfrastructure = Modules.Users.Infrastructure.UsersModule;

namespace Modules.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
    protected const string ModulesNamespace = "Modules";

    protected static readonly ServiceAssemblies Catalog = new(
        "Catalog",
        Modules.Catalog.Domain.AssemblyReference.Assembly,
        Modules.Catalog.Application.AssemblyReference.Assembly,
        Modules.Catalog.Contracts.AssemblyReference.Assembly,
        typeof(Modules.Catalog.Endpoints.EndpointsModule).Assembly,
        typeof(CatalogInfrastructure).Assembly);

    protected static readonly ServiceAssemblies Coupons = new(
        "Coupons",
        Modules.Coupons.Domain.AssemblyReference.Assembly,
        Modules.Coupons.Application.AssemblyReference.Assembly,
        typeof(Modules.Coupons.Contracts.ICouponsPublicApi).Assembly,
        typeof(Modules.Coupons.Endpoints.EndpointsModule).Assembly,
        typeof(CouponsInfrastructure).Assembly);

    protected static readonly ServiceAssemblies Launches = new(
        "Launches",
        Modules.Launches.Domain.AssemblyReference.Assembly,
        Modules.Launches.Application.AssemblyReference.Assembly,
        Modules.Launches.Contracts.AssemblyReference.Assembly,
        typeof(Modules.Launches.Endpoints.EndpointsModule).Assembly,
        typeof(LaunchesInfrastructure).Assembly);

    protected static readonly ServiceAssemblies Orders = new(
        "Orders",
        Modules.Orders.Domain.AssemblyReference.Assembly,
        Modules.Orders.Application.AssemblyReference.Assembly,
        Modules.Orders.Contracts.AssemblyReference.Assembly,
        typeof(Modules.Orders.Endpoints.EndpointsModule).Assembly,
        typeof(OrdersInfrastructure).Assembly);

    protected static readonly ServiceAssemblies Payments = new(
        "Payments",
        Modules.Payments.Domain.AssemblyReference.Assembly,
        Modules.Payments.Application.AssemblyReference.Assembly,
        Modules.Payments.Contracts.AssemblyReference.Assembly,
        typeof(Modules.Payments.Endpoints.EndpointsModule).Assembly,
        typeof(PaymentsInfrastructure).Assembly);

    protected static readonly ServiceAssemblies Users = new(
        "Users",
        Modules.Users.Domain.AssemblyReference.Assembly,
        Modules.Users.Application.AssemblyReference.Assembly,
        Modules.Users.Contracts.AssemblyReference.Assembly,
        typeof(Modules.Users.Endpoints.EndpointsModule).Assembly,
        typeof(UsersInfrastructure).Assembly);

    protected static readonly ServiceAssemblies[] AllServices = [Catalog, Coupons, Launches, Orders, Payments, Users];

    protected sealed record ServiceAssemblies(
        string Name,
        Assembly Domain,
        Assembly Application,
        Assembly Contracts,
        Assembly Endpoints,
        Assembly Infrastructure)
    {
        /// <summary>
        /// The assemblies that make up the private implementation of the service.
        /// Contracts are intentionally excluded because they are the service's public surface.
        /// </summary>
        public Assembly[] InternalAssemblies => [Domain, Application, Endpoints, Infrastructure];

        public string[] InternalAssemblyNames =>
            [.. InternalAssemblies.Select(assembly => assembly.GetName().Name!)];

        /// <summary>
        /// Every assembly owned by the service, public surface included.
        /// </summary>
        public string[] AllAssemblyNames =>
            [.. InternalAssemblyNames, Contracts.GetName().Name!];
    }
}
