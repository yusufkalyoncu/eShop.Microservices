namespace Catalog.API.Features.Products.GetProducts;

public sealed record ProductDto(
    Guid Id, 
    string Name, 
    string Description, 
    decimal PriceAmount, 
    string PriceCurrency, 
    Guid CategoryId);