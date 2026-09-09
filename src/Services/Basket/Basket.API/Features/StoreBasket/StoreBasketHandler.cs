using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Marten;

namespace Basket.API.Features.StoreBasket;

internal sealed class StoreBasketCommandHandler(IDocumentSession session)
    : ICommandHandler<StoreBasketCommand, StoreBasketResult>
{
    public async Task<Result<StoreBasketResult>> Handle(StoreBasketCommand request, CancellationToken cancellationToken)
    {
        session.Store(request.Cart);
        await session.SaveChangesAsync(cancellationToken);

        return new StoreBasketResult(request.Cart.UserName);
    }
}