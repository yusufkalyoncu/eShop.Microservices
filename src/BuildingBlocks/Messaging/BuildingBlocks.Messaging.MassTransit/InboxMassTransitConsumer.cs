using BuildingBlocks.Inbox.Abstractions;
using BuildingBlocks.Messaging.Abstractions;
using MassTransit;

namespace BuildingBlocks.Messaging.MassTransit;

public sealed class InboxMassTransitConsumer<TEvent>(IInboxService inboxService) : IConsumer<TEvent>
    where TEvent : class, IIntegrationEvent
{
    public async Task Consume(ConsumeContext<TEvent> context)
    {
        // We act as a bridge. We simply take the message from the broker 
        // and drop it into the Inbox. The InboxProcessor will handle the rest.
        await inboxService.TryAddAsync(context.Message, partitionKey: null, context.CancellationToken);
    }
}