using BuildingBlocks.Core.DomainEvents;
using BuildingBlocks.Outbox.Abstractions;
using Catalog.API.Domain.Events;
using Catalog.Contracts.IntegrationEvents;

namespace Catalog.API.Features.Products.UpdateProduct;

internal sealed class ProductPriceChangedDomainEventHandler(
    IOutboxService outboxService) : IDomainEventHandler<ProductPriceChangedDomainEvent>
{
    public async Task Handle(
        ProductPriceChangedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new ProductPriceChangedIntegrationEvent(
            Guid.NewGuid(),
            domainEvent.ProductId,
            domainEvent.NewPrice.Amount);

        await outboxService.AddAsync(
            integrationEvent,
            cancellationToken: cancellationToken);
    }
}