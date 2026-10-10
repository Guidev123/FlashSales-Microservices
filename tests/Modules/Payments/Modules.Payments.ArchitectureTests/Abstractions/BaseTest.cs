using System.Reflection;
using ModuleInfrastructure = Modules.Payments.Infrastructure.PaymentsModule;

namespace Modules.Payments.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = Modules.Payments.Domain.AssemblyReference.Assembly;

    protected static readonly Assembly ApplicationAssembly = Modules.Payments.Application.AssemblyReference.Assembly;

    protected static readonly Assembly ContractsAssembly = Modules.Payments.Contracts.AssemblyReference.Assembly;

    protected static readonly Assembly EndpointsAssembly = typeof(Modules.Payments.Endpoints.EndpointsModule).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(ModuleInfrastructure).Assembly;
}
