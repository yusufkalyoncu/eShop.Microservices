using Basket.Contracts.IntegrationEvents;
using BuildingBlocks.Messaging.Abstractions;
using Ordering.API.Domain.Models;
using Ordering.API.Domain.ValueObjects;
using Ordering.API.Infrastructure.Database;
using Ordering.Contracts.IntegrationEvents;

namespace Ordering.API.Consumers;

public class BasketCheckoutIntegrationEventHandler(OrderingDbContext dbContext, IEventBus eventBus)
    : IIntegrationEventHandler<BasketCheckoutIntegrationEvent>
{
    public async Task HandleAsync(BasketCheckoutIntegrationEvent @event, CancellationToken cancellationToken)
    {
        var address = new Address(
            @event.FirstName,
            @event.LastName,
            @event.EmailAddress,
            @event.AddressLine,
            @event.Country,
            @event.State,
            @event.ZipCode);

        var payment = new PaymentDetails(
            @event.CardName,
            @event.CardNumber,
            @event.Expiration,
            @event.CVV,
            @event.PaymentMethod);

        var orderId = Guid.NewGuid();

        var order = Order.Create(
            orderId,
            @event.UserName,
            address, // Using same for shipping and billing for simplicity
            address,
            payment);

        foreach (var item in @event.Items)
        {
            order.Add(item.ProductId, item.ProductName, item.Quantity, item.Price);
        }

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        await eventBus.PublishAsync(new OrderPlacedIntegrationEvent
        {
            OrderId = order.Id,
            UserName = order.UserName,
            OrderStatus = order.Status.ToString()
        }, cancellationToken);
    }
}