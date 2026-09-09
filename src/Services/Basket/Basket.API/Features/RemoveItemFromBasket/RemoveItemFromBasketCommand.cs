using BuildingBlocks.Core.CQRS;

namespace Basket.API.Features.RemoveItemFromBasket;

public record RemoveItemFromBasketCommand(string UserName, Guid ProductId) : ICommand<RemoveItemFromBasketResult>;

public record RemoveItemFromBasketResult(bool IsSuccess);