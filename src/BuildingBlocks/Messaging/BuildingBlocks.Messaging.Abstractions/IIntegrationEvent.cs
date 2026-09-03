namespace BuildingBlocks.Messaging.Abstractions;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    static abstract string EventName { get; }
}