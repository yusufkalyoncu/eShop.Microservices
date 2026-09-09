using Basket.API.Domain.Models;
using BuildingBlocks.Core.CQRS;

namespace Basket.API.Features.StoreBasket;

public record StoreBasketCommand(ShoppingCart Cart) : ICommand<StoreBasketResult>;
public record StoreBasketResult(string UserName);