using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Basket.API.Domain.Models;
using Basket.API.Domain.Errors;
using Marten;

namespace Basket.API.Features.UpdateItemQuantityInBasket;

internal sealed class UpdateItemQuantityInBasketHandler(
    IDocumentSession session)
    : ICommandHandler<UpdateItemQuantityInBasketCommand, UpdateItemQuantityInBasketResult>
{
    public async Task<Result<UpdateItemQuantityInBasketResult>> Handle(UpdateItemQuantityInBasketCommand command, CancellationToken cancellationToken)
    {
        var cart = await session.LoadAsync<ShoppingCart>(command.UserName, cancellationToken);
        
        if (cart is null)
        {
            return Result.Failure<UpdateItemQuantityInBasketResult>(BasketErrors.Cart.NotFound);
        }

        var item = cart.Items.FirstOrDefault(x => x.ProductId == command.ProductId);
        if (item is null)
        {
            return Result.Failure<UpdateItemQuantityInBasketResult>(BasketErrors.Item.NotFound);
        }

        if (command.Quantity <= 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            item.UpdateQuantity(command.Quantity);
        }

        session.Update(cart);
        await session.SaveChangesAsync(cancellationToken);
        
        return new UpdateItemQuantityInBasketResult(true);
    }
}