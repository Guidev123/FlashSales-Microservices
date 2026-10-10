using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Orders.ArchitectureTests.Abstractions;
using NetArchTest.Rules;

namespace Modules.Orders.ArchitectureTests.Infrastructure;

public class InfrastructureTests : BaseTest
{
    [Fact(DisplayName = "Integration Event Handlers Should Not Be Public")]
    [Trait("Orders Architecture Tests", "Infrastructure Tests")]
    public void IntegrationEventHandler_Should_NotBePublic()
    {
        // Arrange & Act
        IEnumerable<Type> failingTypes = NotificationHandlerTypes
            .IntegrationEventHandlers(InfrastructureAssembly)
            .Where(type => type.IsPublic());

        // Assert
        failingTypes.Should().BeEmpty();
    }

    [Fact(DisplayName = "Integration Event Handlers Should Be Sealed")]
    [Trait("Orders Architecture Tests", "Infrastructure Tests")]
    public void IntegrationEventHandler_Should_BeSealed()
    {
        // Arrange & Act
        IEnumerable<Type> failingTypes = NotificationHandlerTypes
            .IntegrationEventHandlers(InfrastructureAssembly)
            .Where(type => !type.IsSealed);

        // Assert
        failingTypes.Should().BeEmpty();
    }

    [Fact(DisplayName = "Integration Event Handlers Should Have Name Ending With IntegrationEventHandler")]
    [Trait("Orders Architecture Tests", "Infrastructure Tests")]
    public void IntegrationEventHandler_ShouldHave_NameEndingWith_IntegrationEventHandler()
    {
        // Arrange & Act
        IEnumerable<Type> failingTypes = NotificationHandlerTypes
            .IntegrationEventHandlers(InfrastructureAssembly)
            .Where(type => !type.Name.EndsWith("IntegrationEventHandler", StringComparison.Ordinal));

        // Assert
        failingTypes.Should().BeEmpty();
    }

    [Fact(DisplayName = "Repositories Should Not Be Public")]
    [Trait("Orders Architecture Tests", "Infrastructure Tests")]
    public void Repository_Should_NotBePublic()
    {
        // Arrange & Act & Assert
        Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .NotBePublic()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Repositories Should Be Sealed")]
    [Trait("Orders Architecture Tests", "Infrastructure Tests")]
    public void Repository_Should_BeSealed()
    {
        // Arrange & Act & Assert
        Types.InAssembly(InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Entity Configurations Should Not Be Public")]
    [Trait("Orders Architecture Tests", "Infrastructure Tests")]
    public void EntityConfiguration_Should_NotBePublic()
    {
        // Arrange & Act & Assert
        Types.InAssembly(InfrastructureAssembly)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Should()
            .NotBePublic()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Entity Configurations Should Be Sealed")]
    [Trait("Orders Architecture Tests", "Infrastructure Tests")]
    public void EntityConfiguration_Should_BeSealed()
    {
        // Arrange & Act & Assert
        Types.InAssembly(InfrastructureAssembly)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Entity Configurations Should Have Name Ending With Configuration")]
    [Trait("Orders Architecture Tests", "Infrastructure Tests")]
    public void EntityConfiguration_ShouldHave_NameEndingWith_Configuration()
    {
        // Arrange & Act & Assert
        Types.InAssembly(InfrastructureAssembly)
            .That()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Should()
            .HaveNameEndingWith("Configuration")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
