using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Marten;

namespace Basket.API.Features.DeleteBasket;

internal sealed class DeleteBasketCommandHandler(IDocumentSession session)
    : ICommandHandler<DeleteBasketCommand, DeleteBasketResult>
{
    public async Task<Result<DeleteBasketResult>> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
    {
        session.Delete<Domain.Models.ShoppingCart>(request.UserName);
        await session.SaveChangesAsync(cancellationToken);

        return new DeleteBasketResult(true);
    }
}