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
    string PaymentToken) : ICommand<CheckoutBasketResult>;

public record CheckoutBasketResult(bool IsSuccess);