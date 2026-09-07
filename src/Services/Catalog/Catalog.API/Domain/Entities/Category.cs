using BuildingBlocks.Core.Domain;

namespace Catalog.API.Domain.Entities;

public sealed class Category : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    private Category() { }

    public static Category Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
        }

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };
    }

    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
        }

        Name = name;
        Description = description;
    }
}