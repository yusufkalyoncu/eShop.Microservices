namespace Catalog.API.Domain.ValueObjects;

public sealed record CategoryDescription
{
    public string Value { get; }

    private CategoryDescription(string value)
    {
        Value = value;
    }

    public static CategoryDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Category description cannot be empty.", nameof(value));
        }

        if (value.Length > 500)
        {
            throw new ArgumentException("Category description cannot exceed 500 characters.", nameof(value));
        }

        return new CategoryDescription(value);
    }
}