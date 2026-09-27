using BuildingBlocks.Core.CQRS;

namespace Basket.API.Features.CheckoutBasket;

public record CheckoutBasketCommand(
    string UserName,
    string FirstName,
    string LastName,
    string EmailAddress,
    string AddressLine,
    string Country,
    string State,
    string ZipCode,
    string CardName,
    string CardNumber,
    string Expiration,
    string CVV,
    int PaymentMethod) : ICommand<CheckoutBasketResult>;

public record CheckoutBasketResult(bool IsSuccess);