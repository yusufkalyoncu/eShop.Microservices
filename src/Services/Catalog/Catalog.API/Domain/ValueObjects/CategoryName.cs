namespace Catalog.API.Domain.ValueObjects;

public sealed record CategoryName
{
    public string Value { get; }

    private CategoryName(string value)
    {
        Value = value;
    }

    public static CategoryName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(value));
        }

        if (value.Length < 3)
        {
            throw new ArgumentException("Category name must be at least 3 characters long.", nameof(value));
        }

        if (value.Length > 50)
        {
            throw new ArgumentException("Category name cannot exceed 50 characters.", nameof(value));
        }

        return new CategoryName(value);
    }
}