namespace Catalog.API.Domain.ValueObjects;

public sealed record ProductName
{
    public string Value { get; }

    private ProductName(string value)
    {
        Value = value;
    }

    public static ProductName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Product name cannot be empty.", nameof(value));
        }

        if (value.Length < 3)
        {
            throw new ArgumentException("Product name must be at least 3 characters long.", nameof(value));
        }

        if (value.Length > 100)
        {
            throw new ArgumentException("Product name cannot exceed 100 characters.", nameof(value));
        }

        return new ProductName(value);
    }
}