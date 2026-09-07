using BuildingBlocks.Core.Domain.Exceptions;
using Catalog.API.Domain.Errors;
using Catalog.API.Domain.ValueObjects;
using FluentAssertions;

namespace Catalog.UnitTests.Domain.ValueObjects;

public class ProductNameTests
{
    [Fact]
    public void Create_WithValidName_ShouldReturnProductName()
    {
        // Arrange
        string name = "iPhone 15";

        // Act
        var productName = ProductName.Create(name);

        // Assert
        productName.Should().NotBeNull();
        productName.Value.Should().Be(name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldThrowDomainException(string emptyName)
    {
        // Act
        var action = () => ProductName.Create(emptyName);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(CatalogErrors.ProductName.Empty.Description);
    }

    [Fact]
    public void Create_WithTooShortName_ShouldThrowDomainException()
    {
        // Arrange
        string shortName = "ab";

        // Act
        var action = () => ProductName.Create(shortName);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(CatalogErrors.ProductName.TooShort.Description);
    }

    [Fact]
    public void Create_WithTooLongName_ShouldThrowDomainException()
    {
        // Arrange
        string longName = new string('a', 101);

        // Act
        var action = () => ProductName.Create(longName);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(CatalogErrors.ProductName.TooLong.Description);
    }
}