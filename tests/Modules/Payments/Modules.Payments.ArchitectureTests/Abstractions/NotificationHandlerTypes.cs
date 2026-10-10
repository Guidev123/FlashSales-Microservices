using FlashSales.Application.Messaging;
using FlashSales.Domain.DomainObjects;
using MidR.Interfaces;
using System.Reflection;

namespace Modules.Payments.ArchitectureTests.Abstractions;

internal static class NotificationHandlerTypes
{
    internal static IReadOnlyList<Type> DomainEventHandlers(Assembly assembly) =>
        HandlersOf(assembly, typeof(DomainEvent));

    internal static IReadOnlyList<Type> IntegrationEventHandlers(Assembly assembly) =>
        HandlersOf(assembly, typeof(IIntegrationEvent));

    internal static bool IsPublic(this Type type) => type.IsPublic || type.IsNestedPublic;

    private static List<Type> HandlersOf(Assembly assembly, Type eventBaseType) =>
        assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .Where(type => type
                .GetInterfaces()
                .Any(i => i.IsGenericType &&
                          i.GetGenericTypeDefinition() == typeof(INotificationHandler<>) &&
                          eventBaseType.IsAssignableFrom(i.GetGenericArguments()[0])))
            .ToList();
}
