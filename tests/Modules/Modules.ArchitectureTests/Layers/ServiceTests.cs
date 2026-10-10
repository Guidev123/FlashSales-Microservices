using Modules.ArchitectureTests.Abstractions;
using NetArchTest.Rules;

namespace Modules.ArchitectureTests.Layers;

public sealed class ServiceTests : BaseTest
{
    [Fact(DisplayName = "Catalog Service Should Only Depend On Contracts Of Other Services")]
    [Trait("Architecture Tests", "Service Tests")]
    public void CatalogService_ShouldOnlyDependOn_ContractsOfOtherServices() =>
        ShouldNotDependOnInternalsOfOtherServices(Catalog);

    [Fact(DisplayName = "Coupons Service Should Only Depend On Contracts Of Other Services")]
    [Trait("Architecture Tests", "Service Tests")]
    public void CouponsService_ShouldOnlyDependOn_ContractsOfOtherServices() =>
        ShouldNotDependOnInternalsOfOtherServices(Coupons);

    [Fact(DisplayName = "Launches Service Should Only Depend On Contracts Of Other Services")]
    [Trait("Architecture Tests", "Service Tests")]
    public void LaunchesService_ShouldOnlyDependOn_ContractsOfOtherServices() =>
        ShouldNotDependOnInternalsOfOtherServices(Launches);

    [Fact(DisplayName = "Orders Service Should Only Depend On Contracts Of Other Services")]
    [Trait("Architecture Tests", "Service Tests")]
    public void OrdersService_ShouldOnlyDependOn_ContractsOfOtherServices() =>
        ShouldNotDependOnInternalsOfOtherServices(Orders);

    [Fact(DisplayName = "Payments Service Should Only Depend On Contracts Of Other Services")]
    [Trait("Architecture Tests", "Service Tests")]
    public void PaymentsService_ShouldOnlyDependOn_ContractsOfOtherServices() =>
        ShouldNotDependOnInternalsOfOtherServices(Payments);

    [Fact(DisplayName = "Users Service Should Only Depend On Contracts Of Other Services")]
    [Trait("Architecture Tests", "Service Tests")]
    public void UsersService_ShouldOnlyDependOn_ContractsOfOtherServices() =>
        ShouldNotDependOnInternalsOfOtherServices(Users);

    [Fact(DisplayName = "Contracts Should Not Have Dependency On Any Other Service")]
    [Trait("Architecture Tests", "Service Tests")]
    public void Contracts_ShouldNotHaveDependencyOn_AnyOtherService()
    {
        foreach (ServiceAssemblies service in AllServices)
        {
            // Arrange
            // Each service publishes its Contracts independently, so they must not even
            // reference the Contracts of another service.
            string[] otherServices = AllServices
                .Where(other => other != service)
                .SelectMany(other => other.AllAssemblyNames)
                .ToArray();

            // Act & Assert
            Types.InAssembly(service.Contracts)
                .Should()
                .NotHaveDependencyOnAny(otherServices)
                .GetResult()
                .ShouldBeSuccessful();
        }
    }

    private static void ShouldNotDependOnInternalsOfOtherServices(ServiceAssemblies service)
    {
        // Arrange
        string[] internalsOfOtherServices = AllServices
            .Where(other => other != service)
            .SelectMany(other => other.InternalAssemblyNames)
            .ToArray();

        // Act & Assert
        Types.InAssemblies(service.InternalAssemblies)
            .Should()
            .NotHaveDependencyOnAny(internalsOfOtherServices)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
