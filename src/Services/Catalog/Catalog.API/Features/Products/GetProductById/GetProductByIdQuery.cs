using BuildingBlocks.Core.CQRS;

namespace Catalog.API.Features.Products.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDetailsDto>;