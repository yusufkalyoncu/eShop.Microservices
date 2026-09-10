using BuildingBlocks.Core.CQRS;

namespace Basket.API.Features.AddItemToBasket;

public record AddItemToBasketCommand(string UserName, Guid ProductId, int Quantity) : ICommand<AddItemToBasketResult>;
public record AddItemToBasketResult(string UserName);