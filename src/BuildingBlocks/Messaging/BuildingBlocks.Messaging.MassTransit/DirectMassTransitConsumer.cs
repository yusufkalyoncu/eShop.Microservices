using BuildingBlocks.Messaging.Abstractions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.MassTransit;

/// <summary>
/// A lightweight MassTransit consumer that dispatches the incoming event
/// directly to the registered <see cref="IIntegrationEventHandler{TEvent}"/>
/// without going through an Inbox. MassTransit's built-in retry / fault
/// policies handle reliability instead.
/// </summary>
public sealed class DirectMassTransitConsumer<TEvent>(IServiceScopeFactory scopeFactory) : IConsumer<TEvent>
    where TEvent : class, IIntegrationEvent
{
    public async Task Consume(ConsumeContext<TEvent> context)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<TEvent>>();
        await handler.HandleAsync(context.Message, context.CancellationToken);
    }
}