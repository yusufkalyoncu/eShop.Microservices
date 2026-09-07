using BuildingBlocks.Core.Domain.Exceptions;
using Catalog.API.Domain.Errors;
using Catalog.API.Domain.ValueObjects;
using FluentAssertions;

namespace Catalog.UnitTests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_ShouldReturnMoney()
    {
        // Arrange
        decimal amount = 100m;
        string currency = "TRY";

        // Act
        var money = Money.Create(amount, currency);

        // Assert
        money.Should().NotBeNull();
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be(currency);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowDomainException()
    {
        // Arrange
        decimal amount = -10m;
        string currency = "TRY";

        // Act
        var action = () => Money.Create(amount, currency);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(CatalogErrors.Money.NegativeAmount.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyCurrency_ShouldThrowDomainException(string invalidCurrency)
    {
        // Arrange
        decimal amount = 10m;

        // Act
        var action = () => Money.Create(amount, invalidCurrency);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(CatalogErrors.Money.EmptyCurrency.Description);
    }
}