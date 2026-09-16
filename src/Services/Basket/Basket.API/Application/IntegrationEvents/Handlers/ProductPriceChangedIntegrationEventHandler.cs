using Basket.API.Domain.Models;
using BuildingBlocks.Messaging.Abstractions;
using Catalog.Contracts.IntegrationEvents;
using Marten;

namespace Basket.API.Application.IntegrationEvents.Handlers;

public sealed class ProductPriceChangedIntegrationEventHandler(
    IDocumentSession session) : IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>
{
    public async Task HandleAsync(
        ProductPriceChangedIntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        var basketsWithProduct = session.Query<ShoppingCart>()
            .Where(b => b.Items.Any(i => i.ProductId == @event.ProductId))
            .ToAsyncEnumerable(cancellationToken);
        
        int batchSize = 0;

        await foreach (var basket in basketsWithProduct)
        {
            var item = basket.Items.FirstOrDefault(i => i.ProductId == @event.ProductId);
            
            if (item != null && item.Price != @event.NewPrice)
            {
                item.UpdatePrice(@event.NewPrice);
                session.Store(basket);
                batchSize++;
            }

            if (batchSize >= 100)
            {
                await session.SaveChangesAsync(cancellationToken);
                batchSize = 0;
            }
        }

        if (batchSize > 0)
        {
            await session.SaveChangesAsync(cancellationToken);
        }
    }
}