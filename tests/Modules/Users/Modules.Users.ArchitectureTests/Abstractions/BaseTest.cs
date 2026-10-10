using System.Reflection;
using ModuleInfrastructure = Modules.Users.Infrastructure.UsersModule;

namespace Modules.Users.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = Modules.Users.Domain.AssemblyReference.Assembly;

    protected static readonly Assembly ApplicationAssembly = Modules.Users.Application.AssemblyReference.Assembly;

    protected static readonly Assembly ContractsAssembly = Modules.Users.Contracts.AssemblyReference.Assembly;

    protected static readonly Assembly EndpointsAssembly = typeof(Modules.Users.Endpoints.EndpointsModule).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(ModuleInfrastructure).Assembly;
}
