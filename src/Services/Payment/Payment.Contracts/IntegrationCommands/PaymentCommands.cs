using BuildingBlocks.Messaging.Abstractions;

namespace Payment.Contracts.IntegrationCommands;

public class ProcessPaymentIntegrationCommand : IIntegrationEvent 
{
    public Guid EventId { get; } = Guid.NewGuid();
    public static string EventName => "ProcessPaymentIntegrationCommand";
    
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string PaymentToken { get; init; } = string.Empty;
}