using BuildingBlocks.Messaging.Abstractions;

namespace Ordering.Contracts.IntegrationEvents;

public class OrderPlacedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public static string EventName => "OrderPlacedIntegrationEvent";

    public Guid OrderId { get; set; }
    public string UserName { get; set; } = null!;
    public string OrderStatus { get; set; } = null!;
}