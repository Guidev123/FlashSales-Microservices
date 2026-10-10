using Modules.Launches.ArchitectureTests.Abstractions;
using NetArchTest.Rules;

namespace Modules.Launches.ArchitectureTests.Layers;

public class LayerTests : BaseTest
{
    [Fact(DisplayName = "Domain Layer Should Not Have Dependency On Application Layer")]
    [Trait("Launches Architecture Tests", "Layers Tests")]
    public void DomainLayer_ShouldNotHaveDependencyOn_ApplicationLayer()
    {
        // Arrange & Act & Assert
        Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(ApplicationAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Domain Layer Should Not Have Dependency On Contracts Layer")]
    [Trait("Launches Architecture Tests", "Layers Tests")]
    public void DomainLayer_ShouldNotHaveDependencyOn_ContractsLayer()
    {
        // Arrange & Act & Assert
        Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(ContractsAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Domain Layer Should Not Have Dependency On Endpoints Layer")]
    [Trait("Launches Architecture Tests", "Layers Tests")]
    public void DomainLayer_ShouldNotHaveDependencyOn_EndpointsLayer()
    {
        // Arrange & Act & Assert
        Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(EndpointsAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Domain Layer Should Not Have Dependency On Infrastructure Layer")]
    [Trait("Launches Architecture Tests", "Layers Tests")]
    public void DomainLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        // Arrange & Act & Assert
        Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Application Layer Should Not Have Dependency On Endpoints Layer")]
    [Trait("Launches Architecture Tests", "Layers Tests")]
    public void ApplicationLayer_ShouldNotHaveDependencyOn_EndpointsLayer()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(EndpointsAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Application Layer Should Not Have Dependency On Infrastructure Layer")]
    [Trait("Launches Architecture Tests", "Layers Tests")]
    public void ApplicationLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Endpoints Layer Should Not Have Dependency On Infrastructure Layer")]
    [Trait("Launches Architecture Tests", "Layers Tests")]
    public void EndpointsLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        // Arrange & Act & Assert
        Types.InAssembly(EndpointsAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Contracts Layer Should Not Have Dependency On Any Other Layer Of The Module")]
    [Trait("Launches Architecture Tests", "Layers Tests")]
    public void ContractsLayer_ShouldNotHaveDependencyOn_AnyOtherLayer()
    {
        // Arrange
        string[] otherLayers =
        [
            DomainAssembly.GetName().Name!,
            ApplicationAssembly.GetName().Name!,
            EndpointsAssembly.GetName().Name!,
            InfrastructureAssembly.GetName().Name!
        ];

        // Act & Assert
        Types.InAssembly(ContractsAssembly)
            .Should()
            .NotHaveDependencyOnAny(otherLayers)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
