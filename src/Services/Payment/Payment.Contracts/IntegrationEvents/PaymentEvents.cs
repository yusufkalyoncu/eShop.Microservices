using BuildingBlocks.Messaging.Abstractions;

namespace Payment.Contracts.IntegrationEvents;

public class PaymentSucceededEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public static string EventName => "PaymentSucceededEvent";
    
    public Guid OrderId { get; init; }
}

public class PaymentFailedEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public static string EventName => "PaymentFailedEvent";
    
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}