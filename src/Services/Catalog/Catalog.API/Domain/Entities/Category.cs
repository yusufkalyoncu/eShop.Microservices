using BuildingBlocks.Core.Domain;
using Catalog.API.Domain.ValueObjects;

namespace Catalog.API.Domain.Entities;

public sealed class Category : AggregateRoot<Guid>
{
    public CategoryName Name { get; private set; } = null!;
    public CategoryDescription Description { get; private set; } = null!;

    private Category() { }

    public static Category Create(CategoryName name, CategoryDescription description)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };
    }

    public void Update(CategoryName name, CategoryDescription description)
    {
        Name = name;
        Description = description;
    }
}