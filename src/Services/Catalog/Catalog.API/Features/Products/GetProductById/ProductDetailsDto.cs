namespace Catalog.API.Features.Products.GetProductById;

public sealed record ProductDetailsDto(
    Guid Id, 
    string Name, 
    string Description, 
    decimal PriceAmount, 
    string PriceCurrency, 
    Guid CategoryId,
    string CategoryName);