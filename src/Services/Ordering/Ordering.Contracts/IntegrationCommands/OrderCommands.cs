using BuildingBlocks.Messaging.Abstractions;

namespace Ordering.Contracts.IntegrationCommands;

public class CompleteOrderCommand : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public static string EventName => "CompleteOrderCommand";
    
    public Guid OrderId { get; init; }
}

public class CancelOrderCommand : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public static string EventName => "CancelOrderCommand";
    
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}