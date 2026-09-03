using BuildingBlocks.Core.Domain;

namespace Catalog.API.Domain.Entities;

public sealed class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Price { get; private set; }

    // EF Core constructor
    private Product() { }

    public static Product Create(string name, string description, decimal price)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price
        };
    }
}