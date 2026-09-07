namespace Catalog.API.Domain.ValueObjects;

public sealed record ProductDescription
{
    public string Value { get; }

    private ProductDescription(string value)
    {
        Value = value;
    }

    public static ProductDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Product description cannot be empty.", nameof(value));
        }

        if (value.Length > 500)
        {
            throw new ArgumentException("Product description cannot exceed 500 characters.", nameof(value));
        }

        return new ProductDescription(value);
    }
}