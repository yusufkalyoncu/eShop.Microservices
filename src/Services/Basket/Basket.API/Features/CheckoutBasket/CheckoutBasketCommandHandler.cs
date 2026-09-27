using Basket.API.Domain.Errors;
using Basket.API.Domain.Models;
using Basket.Contracts.IntegrationEvents;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using MassTransit;
using Marten;

namespace Basket.API.Features.CheckoutBasket;

public class CheckoutBasketCommandHandler(IDocumentSession session, IPublishEndpoint publishEndpoint)
    : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<Result<CheckoutBasketResult>> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        var userName = request.UserName;

        var basket = await session.LoadAsync<ShoppingCart>(userName, cancellationToken);
        if (basket == null || basket.Items.Count == 0)
        {
            return Result.Failure<CheckoutBasketResult>(BasketErrors.Cart.Empty);
        }

        var eventMessage = new BasketCheckoutIntegrationEvent
        {
            UserName = userName,
            TotalPrice = basket.TotalPrice,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailAddress = request.EmailAddress,
            AddressLine = request.AddressLine,
            Country = request.Country,
            State = request.State,
            ZipCode = request.ZipCode,
            CardName = request.CardName,
            CardNumber = request.CardNumber,
            Expiration = request.Expiration,
            CVV = request.CVV,
            PaymentMethod = request.PaymentMethod,
            Items = basket.Items.Select(x => new BasketItemDto
            {
                ProductId = x.ProductId,
                ProductName = x.ProductName,
                Price = x.Price,
                Quantity = x.Quantity
            }).ToList()
        };

        await publishEndpoint.Publish(eventMessage, cancellationToken);

        session.Delete(basket);
        await session.SaveChangesAsync(cancellationToken);

        return Result.Success(new CheckoutBasketResult(true));
    }
}