using BuildingBlocks.Core.Domain.Exceptions;
using Catalog.API.Domain.Errors;

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
            throw new DomainException(CatalogErrors.ProductDescription.Empty);
        }

        if (value.Length > 500)
        {
            throw new DomainException(CatalogErrors.ProductDescription.TooLong);
        }

        return new ProductDescription(value);
    }
}