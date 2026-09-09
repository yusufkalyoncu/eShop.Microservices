using Basket.API.Domain.Models;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Marten;

namespace Basket.API.Features.GetBasket;

internal sealed class GetBasketQueryHandler(IDocumentSession session) : IQueryHandler<GetBasketQuery, GetBasketResult>
{
    public async Task<Result<GetBasketResult>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = await session.LoadAsync<ShoppingCart>(request.UserName, cancellationToken);
        
        // Return a new basket if not found
        basket ??= new ShoppingCart(request.UserName);

        return new GetBasketResult(basket);
    }
}