namespace Catalog.API.Features.Products.UpdateProduct;

public sealed record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string Currency,
    Guid CategoryId);