using BuildingBlocks.Messaging.Abstractions;

namespace Basket.Contracts.IntegrationEvents;

public class BasketCheckoutIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public static string EventName => "BasketCheckoutIntegrationEvent";

    public string UserName { get; init; } = null!;
    public decimal TotalPrice { get; set; }

    // Shipping and Billing
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string EmailAddress { get; init; } = null!;
    public string AddressLine { get; init; } = null!;
    public string Country { get; init; } = null!;
    public string State { get; init; } = null!;
    public string ZipCode { get; init; } = null!;

    // Payment
    public string CardName { get; init; } = null!;
    public string CardNumber { get; init; } = null!;
    public string Expiration { get; init; } = null!;
    public string CVV { get; init; } = null!;
    public int PaymentMethod { get; init; }

    public List<BasketItemDto> Items { get; init; } = [];
}

public class BasketItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = null!;
    public int Quantity { get; init; }
    public decimal Price { get; init; }
}