using FlashSales.Application.Messaging;
using FlashSales.Domain.DomainObjects;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure.Authentication;
using Modules.ArchitectureTests.Abstractions;
using NetArchTest.Rules;
using System.Reflection;

namespace Modules.ArchitectureTests.Layers;

public sealed class BuildingBlocksTests : BaseTest
{
    private static readonly Assembly DomainAssembly = typeof(Entity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(ICommand).Assembly;
    private static readonly Assembly EndpointsAssembly = typeof(IEndpoint).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(ClaimsPrincipalExtensions).Assembly;

    [Fact(DisplayName = "Building Blocks Domain Should Not Have Dependency On Other Building Blocks")]
    [Trait("Architecture Tests", "Building Blocks Tests")]
    public void BuildingBlocksDomain_ShouldNotHaveDependencyOn_OtherBuildingBlocks()
    {
        // Arrange & Act & Assert
        Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                ApplicationAssembly.GetName().Name!,
                EndpointsAssembly.GetName().Name!,
                InfrastructureAssembly.GetName().Name!)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Building Blocks Application Should Not Have Dependency On Endpoints Or Infrastructure")]
    [Trait("Architecture Tests", "Building Blocks Tests")]
    public void BuildingBlocksApplication_ShouldNotHaveDependencyOn_EndpointsOrInfrastructure()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                EndpointsAssembly.GetName().Name!,
                InfrastructureAssembly.GetName().Name!)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Building Blocks Endpoints Should Not Have Dependency On Infrastructure")]
    [Trait("Architecture Tests", "Building Blocks Tests")]
    public void BuildingBlocksEndpoints_ShouldNotHaveDependencyOn_Infrastructure()
    {
        // Arrange & Act & Assert
        Types.InAssembly(EndpointsAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Building Blocks Should Not Have Dependency On Any Service")]
    [Trait("Architecture Tests", "Building Blocks Tests")]
    public void BuildingBlocks_ShouldNotHaveDependencyOn_AnyService()
    {
        // Arrange & Act & Assert
        Types.InAssemblies([DomainAssembly, ApplicationAssembly, EndpointsAssembly, InfrastructureAssembly])
            .Should()
            .NotHaveDependencyOn(ModulesNamespace)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
