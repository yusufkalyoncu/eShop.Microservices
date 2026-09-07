using BuildingBlocks.Core.Domain;
using Catalog.API.Domain.ValueObjects;

namespace Catalog.API.Domain.Entities;

public sealed class Product : AggregateRoot<Guid>
{
    public ProductName Name { get; private set; } = null!;
    public ProductDescription Description { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public Guid CategoryId { get; private set; }

    // EF Core constructor
    private Product() { }

    public static Product Create(ProductName name, ProductDescription description, Money price, Guid categoryId)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            CategoryId = categoryId
        };
    }

    public void UpdatePrice(Money newPrice) => Price = newPrice;
    public void ChangeCategory(Guid newCategoryId) => CategoryId = newCategoryId;
}