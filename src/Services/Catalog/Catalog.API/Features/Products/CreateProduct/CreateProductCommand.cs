using BuildingBlocks.Core.CQRS;

namespace Catalog.API.Features.Products.CreateProduct;

public sealed record CreateProductCommand(string Name, string Description, decimal Price, Guid CategoryId) : ICommand<Guid>;