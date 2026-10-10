using System.Reflection;
using ModuleInfrastructure = Modules.Catalog.Infrastructure.CatalogModule;

namespace Modules.Catalog.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = Modules.Catalog.Domain.AssemblyReference.Assembly;

    protected static readonly Assembly ApplicationAssembly = Modules.Catalog.Application.AssemblyReference.Assembly;

    protected static readonly Assembly ContractsAssembly = Modules.Catalog.Contracts.AssemblyReference.Assembly;

    protected static readonly Assembly EndpointsAssembly = typeof(Modules.Catalog.Endpoints.EndpointsModule).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(ModuleInfrastructure).Assembly;
}
