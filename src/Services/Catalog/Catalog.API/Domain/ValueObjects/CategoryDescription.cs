using BuildingBlocks.Core.Domain.Exceptions;
using Catalog.API.Domain.Errors;

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
            throw new DomainException(CatalogErrors.CategoryDescription.Empty);
        }

        if (value.Length > 500)
        {
            throw new DomainException(CatalogErrors.CategoryDescription.TooLong);
        }

        return new CategoryDescription(value);
    }
}