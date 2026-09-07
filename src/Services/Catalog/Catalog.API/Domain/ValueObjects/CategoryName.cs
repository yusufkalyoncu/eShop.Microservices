using BuildingBlocks.Core.Domain.Exceptions;
using Catalog.API.Domain.Errors;

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
            throw new DomainException(CatalogErrors.CategoryName.Empty);
        }

        if (value.Length < 3)
        {
            throw new DomainException(CatalogErrors.CategoryName.TooShort);
        }

        if (value.Length > 50)
        {
            throw new DomainException(CatalogErrors.CategoryName.TooLong);
        }

        return new CategoryName(value);
    }
}