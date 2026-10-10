using System.Reflection;
using ModuleInfrastructure = Modules.Coupons.Infrastructure.CouponsModule;

namespace Modules.Coupons.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Assembly DomainAssembly = Modules.Coupons.Domain.AssemblyReference.Assembly;

    protected static readonly Assembly ApplicationAssembly = Modules.Coupons.Application.AssemblyReference.Assembly;

    protected static readonly Assembly ContractsAssembly = typeof(Modules.Coupons.Contracts.ICouponsPublicApi).Assembly;

    protected static readonly Assembly EndpointsAssembly = typeof(Modules.Coupons.Endpoints.EndpointsModule).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(ModuleInfrastructure).Assembly;
}
