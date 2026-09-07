using BuildingBlocks.Core.CQRS;

namespace Catalog.API.Features.Products.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : ICommand;