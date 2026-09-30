using BuildingBlocks.Messaging.Abstractions;

namespace Inventory.Contracts.IntegrationEvents;

public class StockReservedEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public static string EventName => "StockReservedEvent";
    
    public Guid OrderId { get; init; }
}

public class StockReservationFailedEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public static string EventName => "StockReservationFailedEvent";
    
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}