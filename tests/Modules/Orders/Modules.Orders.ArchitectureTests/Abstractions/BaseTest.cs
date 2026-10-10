using System.Reflection;
using ModuleInfrastructure = Modules.Orders.Infrastructure.OrdersModule;

namespace Modules.Orders.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = Modules.Orders.Domain.AssemblyReference.Assembly;

    protected static readonly Assembly ApplicationAssembly = Modules.Orders.Application.AssemblyReference.Assembly;

    protected static readonly Assembly ContractsAssembly = Modules.Orders.Contracts.AssemblyReference.Assembly;

    protected static readonly Assembly EndpointsAssembly = typeof(Modules.Orders.Endpoints.EndpointsModule).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(ModuleInfrastructure).Assembly;
}
