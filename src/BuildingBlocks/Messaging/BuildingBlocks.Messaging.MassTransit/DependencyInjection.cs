using System.Reflection;
using BuildingBlocks.Messaging.Abstractions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.MassTransit;

public static class DependencyInjection
{
    public static IServiceCollection AddMassTransitEventBus(
        this IServiceCollection services,
        Assembly[] scanAssemblies,
        Action<IBusRegistrationConfigurator>? configure = null)
    {
        services.AddSingleton<IEventBus, MassTransitEventBus>();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            // 1. Find all handler types and auto-register them in DI
            var handlerTypes = scanAssemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Select(t => new
                {
                    ImplementationType = t,
                    HandlerInterfaces = t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
                        .ToList()
                })
                .Where(x => x.HandlerInterfaces.Count > 0)
                .ToList();

            foreach (var handlerInfo in handlerTypes)
            {
                foreach (var handlerInterface in handlerInfo.HandlerInterfaces)
                {
                    // Register the handler in DI so InboxProcessor can resolve it
                    services.AddScoped(handlerInterface, handlerInfo.ImplementationType);
                }
            }

            // 2. Register InboxMassTransitConsumer<T> for each distinct T
            var eventTypes = handlerTypes
                .SelectMany(h => h.HandlerInterfaces)
                .Select(i => i.GetGenericArguments()[0])
                .Distinct()
                .ToList();

            foreach (var consumerType in eventTypes.Select(eventType => typeof(InboxMassTransitConsumer<>).MakeGenericType(eventType)))
            {
                x.AddConsumer(consumerType);
            }

            configure?.Invoke(x);
        });

        return services;
    }
}