using FlashSales.Domain.DomainObjects;
using FluentAssertions;
using Modules.Launches.ArchitectureTests.Abstractions;
using NetArchTest.Rules;
using System.Reflection;

namespace Modules.Launches.ArchitectureTests.Domain;

public class DomainTests : BaseTest
{
    [Fact(DisplayName = "Domain Events Should Be Sealed")]
    [Trait("Launches Architecture Tests", "Domain Tests")]
    public void DomainEvent_Should_BeSealed()
    {
        // Arrange & Act & Assert
        Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(DomainEvent))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Domain Events Should Have Name Ending With DomainEvent")]
    [Trait("Launches Architecture Tests", "Domain Tests")]
    public void DomainEvent_ShouldHave_NameEndingWith_DomainEvent()
    {
        // Arrange & Act & Assert
        Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(DomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Entities Should Be Sealed")]
    [Trait("Launches Architecture Tests", "Domain Tests")]
    public void Entities_Should_BeSealed()
    {
        // Arrange & Act & Assert
        Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Entities Should Have Private Parameterless Constructor")]
    [Trait("Launches Architecture Tests", "Domain Tests")]
    public void Entities_ShouldHave_PrivateParameterlessConstructor()
    {
        // Arrange & Act
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes();

        var failingTypes = new List<Type>();
        foreach (Type entityType in entityTypes)
        {
            ConstructorInfo[] constructors = entityType.GetConstructors(BindingFlags.NonPublic |
                                                                        BindingFlags.Instance);

            if (!constructors.Any(c => c.IsPrivate && c.GetParameters().Length == 0))
            {
                failingTypes.Add(entityType);
            }
        }

        // Assert
        failingTypes.Should().BeEmpty();
    }

    [Fact(DisplayName = "Entities Should Only Have Private Constructors")]
    [Trait("Launches Architecture Tests", "Domain Tests")]
    public void Entities_ShouldOnlyHave_PrivateConstructors()
    {
        // Arrange & Act
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes();

        var failingTypes = new List<Type>();
        foreach (Type entityType in entityTypes)
        {
            ConstructorInfo[] constructors = entityType.GetConstructors(BindingFlags.Public |
                                                                        BindingFlags.Instance);

            if (constructors.Any())
            {
                failingTypes.Add(entityType);
            }
        }

        // Assert
        failingTypes.Should().BeEmpty();
    }
}
