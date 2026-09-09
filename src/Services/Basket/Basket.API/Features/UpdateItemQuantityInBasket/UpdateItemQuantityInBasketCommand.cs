using BuildingBlocks.Core.CQRS;

namespace Basket.API.Features.UpdateItemQuantityInBasket;

public record UpdateItemQuantityInBasketCommand(string UserName, Guid ProductId, int Quantity) : ICommand<UpdateItemQuantityInBasketResult>;

public record UpdateItemQuantityInBasketResult(bool IsSuccess);