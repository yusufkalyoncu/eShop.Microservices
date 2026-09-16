using BuildingBlocks.Messaging.Abstractions;

namespace Catalog.Contracts.IntegrationEvents;

public sealed record ProductPriceChangedIntegrationEvent(
    Guid EventId,
    Guid ProductId,
    decimal NewPrice) : IIntegrationEvent
{
    public static string EventName => "catalog.product-price-changed";
}