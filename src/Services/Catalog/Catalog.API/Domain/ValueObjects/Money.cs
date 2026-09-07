using BuildingBlocks.Core.Domain.Exceptions;
using Catalog.API.Domain.Errors;

namespace Catalog.API.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "TRY")
    {
        if (amount < 0)
        {
            throw new DomainException(CatalogErrors.Money.NegativeAmount);
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException(CatalogErrors.Money.EmptyCurrency);
        }

        return new Money(amount, currency);
    }
}