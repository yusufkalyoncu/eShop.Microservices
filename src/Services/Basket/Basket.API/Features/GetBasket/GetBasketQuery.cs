using Basket.API.Domain.Models;
using BuildingBlocks.Core.CQRS;

namespace Basket.API.Features.GetBasket;

public record GetBasketQuery(string UserName) : IQuery<GetBasketResult>;
public record GetBasketResult(ShoppingCart Cart);