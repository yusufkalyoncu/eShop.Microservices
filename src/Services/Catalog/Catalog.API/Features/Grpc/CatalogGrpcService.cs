using Catalog.API.Infrastructure.Data;
using Catalog.Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Features.Grpc;

public class CatalogGrpcService(CatalogDbContext dbContext) : global::Catalog.Grpc.CatalogGrpcService.CatalogGrpcServiceBase
{
    public override async Task<GetProductByIdResponse> GetProductById(GetProductByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var productId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid product ID format."));
        }

        var product = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, context.CancellationToken);

        if (product == null)
        {
            // The GlobalExceptionInterceptor maps DomainException.NotFound to StatusCode.NotFound, 
            // but we can also throw RpcException directly if preferred.
            throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.Id} was not found."));
        }

        return new GetProductByIdResponse
        {
            Id = product.Id.ToString(),
            Name = product.Name.Value,
            Price = (double)product.Price.Amount
        };
    }
}
