using BuildingBlocks.Core.CQRS;

namespace Catalog.API.Features.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id, 
    string Name, 
    string Description, 
    decimal Price, 
    string Currency, 
    Guid CategoryId) : ICommand;