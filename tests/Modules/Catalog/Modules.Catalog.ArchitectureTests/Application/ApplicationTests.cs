using FlashSales.Application.Messaging;
using FluentAssertions;
using FluentValidation;
using Modules.Catalog.ArchitectureTests.Abstractions;
using NetArchTest.Rules;

namespace Modules.Catalog.ArchitectureTests.Application;

public class ApplicationTests : BaseTest
{
    [Fact(DisplayName = "Commands Should Be Sealed")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void Command_Should_BeSealed()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or()
            .ImplementInterface(typeof(ICommand<>))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Commands Should Have Name Ending With Command")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void Command_ShouldHave_NameEndingWith_Command()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or()
            .ImplementInterface(typeof(ICommand<>))
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Command Handlers Should Not Be Public")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void CommandHandler_Should_NotBePublic()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Command Handlers Should Be Sealed")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void CommandHandler_Should_BeSealed()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Command Handlers Should Have Name Ending With CommandHandler")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void CommandHandler_ShouldHave_NameEndingWith_CommandHandler()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Queries Should Be Sealed")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void Query_Should_BeSealed()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Queries Should Have Name Ending With Query")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void Query_ShouldHave_NameEndingWith_Query()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Query Handlers Should Not Be Public")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void QueryHandler_Should_NotBePublic()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Query Handlers Should Be Sealed")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void QueryHandler_Should_BeSealed()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Query Handlers Should Have Name Ending With QueryHandler")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void QueryHandler_ShouldHave_NameEndingWith_QueryHandler()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Validators Should Not Be Public")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void Validator_Should_NotBePublic()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .NotBePublic()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Validators Should Be Sealed")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void Validator_Should_BeSealed()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Validators Should Have Name Ending With Validator")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void Validator_ShouldHave_NameEndingWith_Validator()
    {
        // Arrange & Act & Assert
        Types.InAssembly(ApplicationAssembly)
            .That()
            .Inherit(typeof(AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact(DisplayName = "Domain Event Handlers Should Not Be Public")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void DomainEventHandler_Should_NotBePublic()
    {
        // Arrange & Act
        IEnumerable<Type> failingTypes = NotificationHandlerTypes
            .DomainEventHandlers(ApplicationAssembly)
            .Where(type => type.IsPublic());

        // Assert
        failingTypes.Should().BeEmpty();
    }

    [Fact(DisplayName = "Domain Event Handlers Should Be Sealed")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void DomainEventHandler_Should_BeSealed()
    {
        // Arrange & Act
        IEnumerable<Type> failingTypes = NotificationHandlerTypes
            .DomainEventHandlers(ApplicationAssembly)
            .Where(type => !type.IsSealed);

        // Assert
        failingTypes.Should().BeEmpty();
    }

    [Fact(DisplayName = "Domain Event Handlers Should Have Name Ending With DomainEventHandler")]
    [Trait("Catalog Architecture Tests", "Application Tests")]
    public void DomainEventHandler_ShouldHave_NameEndingWith_DomainEventHandler()
    {
        // Arrange & Act
        IEnumerable<Type> failingTypes = NotificationHandlerTypes
            .DomainEventHandlers(ApplicationAssembly)
            .Where(type => !type.Name.EndsWith("DomainEventHandler", StringComparison.Ordinal));

        // Assert
        failingTypes.Should().BeEmpty();
    }
}
