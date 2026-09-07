using BuildingBlocks.Core.Domain.Exceptions;
using Catalog.API.Domain.Errors;

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
            throw new DomainException(CatalogErrors.ProductName.Empty);
        }

        if (value.Length < 3)
        {
            throw new DomainException(CatalogErrors.ProductName.TooShort);
        }

        if (value.Length > 100)
        {
            throw new DomainException(CatalogErrors.ProductName.TooLong);
        }

        return new ProductName(value);
    }
}