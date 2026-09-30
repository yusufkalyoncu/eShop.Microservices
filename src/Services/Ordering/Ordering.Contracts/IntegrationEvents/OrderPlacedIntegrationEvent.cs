using BuildingBlocks.Messaging.Abstractions;

namespace Ordering.Contracts.IntegrationEvents;

public class OrderPlacedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public static string EventName => "OrderPlacedIntegrationEvent";

    public Guid OrderId { get; init; }
    public string UserName { get; init; } = null!;
    public string OrderStatus { get; init; } = null!;
    public decimal TotalAmount { get; init; }
    public string PaymentToken { get; init; } = null!;
    public Dictionary<Guid, int> Items { get; init; } = new();
}