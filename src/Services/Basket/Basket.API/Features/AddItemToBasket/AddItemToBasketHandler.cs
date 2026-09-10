using Basket.API.Domain.Errors;
using Basket.API.Domain.Models;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Catalog.Grpc;
using Grpc.Core;
using Marten;
using BuildingBlocks.Core.Domain.Exceptions;

namespace Basket.API.Features.AddItemToBasket;

internal sealed class AddItemToBasketCommandHandler(
    IDocumentSession session,
    CatalogGrpcService.CatalogGrpcServiceClient catalogClient)
    : ICommandHandler<AddItemToBasketCommand, AddItemToBasketResult>
{
    public async Task<Result<AddItemToBasketResult>> Handle(AddItemToBasketCommand request, CancellationToken cancellationToken)
    {
        GetProductByIdResponse productResponse;
        try
        {
            productResponse = await catalogClient.GetProductByIdAsync(
                new GetProductByIdRequest { Id = request.ProductId.ToString() }, 
                cancellationToken: cancellationToken);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new DomainException(BasketErrors.Product.NotFound(request.ProductId));
        }
        
        var cart = await session.LoadAsync<ShoppingCart>(request.UserName, cancellationToken);
        if (cart == null)
        {
            cart = new ShoppingCart(request.UserName);
        }
        
        cart.AddItem(
            request.ProductId,
            productResponse.Name,
            request.Quantity,
            (decimal)productResponse.Price);
            
        session.Store(cart);
        await session.SaveChangesAsync(cancellationToken);
        
        return new AddItemToBasketResult(cart.UserName);
    }
}