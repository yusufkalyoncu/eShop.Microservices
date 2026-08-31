namespace BuildingBlocks.Outbox.Abstractions;

public interface IOutboxEvent
{
    static abstract string EventName { get; }
}