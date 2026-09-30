using BuildingBlocks.Messaging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Infrastructure.Database;
using Ordering.Contracts.IntegrationCommands;

namespace Ordering.API.Consumers;

public class CompleteOrderConsumer(OrderingDbContext dbContext) : IIntegrationEventHandler<CompleteOrderCommand>
{
    public async Task HandleAsync(CompleteOrderCommand @event, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == @event.OrderId, cancellationToken);
        if (order != null)
        {
            order.MarkAsPaid();
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

public class CancelOrderConsumer(OrderingDbContext dbContext) : IIntegrationEventHandler<CancelOrderCommand>
{
    public async Task HandleAsync(CancelOrderCommand @event, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == @event.OrderId, cancellationToken);
        if (order != null)
        {
            order.MarkAsCancelled();
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}