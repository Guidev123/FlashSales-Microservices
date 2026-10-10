using FlashSales.Endpoints.Endpoints;
using Modules.Users.ArchitectureTests.Abstractions;
using NetArchTest.Rules;

namespace Modules.Users.ArchitectureTests.Endpoints;

public class EndpointsTests : BaseTest
{
    [Fact(DisplayName = "Endpoints Should Not Be Public")]
    [Trait("Users Architecture Tests", "Endpoints Tests")]
    public void Endpoint_Should_NotBePublic()
    {
        // Arrange & Act & Assert
        Types.InAssembly(EndpointsAssembly)
            .That()
            .ImplementInterface(typeof(IEndpoint))
            .Should()
            .NotBePublic()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Endpoints Should Be Sealed")]
    [Trait("Users Architecture Tests", "Endpoints Tests")]
    public void Endpoint_Should_BeSealed()
    {
        // Arrange & Act & Assert
        Types.InAssembly(EndpointsAssembly)
            .That()
            .ImplementInterface(typeof(IEndpoint))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Endpoints Should Have Name Ending With Endpoint")]
    [Trait("Users Architecture Tests", "Endpoints Tests")]
    public void Endpoint_ShouldHave_NameEndingWith_Endpoint()
    {
        // Arrange & Act & Assert
        Types.InAssembly(EndpointsAssembly)
            .That()
            .ImplementInterface(typeof(IEndpoint))
            .Should()
            .HaveNameEndingWith("Endpoint")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
