using System.Reflection;
using ModuleInfrastructure = Modules.Launches.Infrastructure.LaunchesModule;

namespace Modules.Launches.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = Modules.Launches.Domain.AssemblyReference.Assembly;

    protected static readonly Assembly ApplicationAssembly = Modules.Launches.Application.AssemblyReference.Assembly;

    protected static readonly Assembly ContractsAssembly = Modules.Launches.Contracts.AssemblyReference.Assembly;

    protected static readonly Assembly EndpointsAssembly = typeof(Modules.Launches.Endpoints.EndpointsModule).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(ModuleInfrastructure).Assembly;
}
