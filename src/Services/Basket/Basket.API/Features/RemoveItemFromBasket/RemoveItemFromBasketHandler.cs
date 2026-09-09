using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Basket.API.Domain.Models;
using Basket.API.Domain.Errors;
using Marten;

namespace Basket.API.Features.RemoveItemFromBasket;

internal sealed class RemoveItemFromBasketHandler(
    IDocumentSession session)
    : ICommandHandler<RemoveItemFromBasketCommand, RemoveItemFromBasketResult>
{
    public async Task<Result<RemoveItemFromBasketResult>> Handle(RemoveItemFromBasketCommand command, CancellationToken cancellationToken)
    {
        var cart = await session.LoadAsync<ShoppingCart>(command.UserName, cancellationToken);
        
        if (cart is null)
        {
            return Result.Failure<RemoveItemFromBasketResult>(BasketErrors.Cart.NotFound);
        }

        var itemToRemove = cart.Items.FirstOrDefault(x => x.ProductId == command.ProductId);
        if (itemToRemove is not null)
        {
            cart.Items.Remove(itemToRemove);
            
            session.Update(cart);
            await session.SaveChangesAsync(cancellationToken);
            
            return new RemoveItemFromBasketResult(true);
        }

        return new RemoveItemFromBasketResult(false);
    }
}
