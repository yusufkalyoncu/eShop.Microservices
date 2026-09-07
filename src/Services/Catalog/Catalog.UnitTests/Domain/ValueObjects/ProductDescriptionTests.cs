using BuildingBlocks.Core.Domain.Exceptions;
using Catalog.API.Domain.Errors;
using Catalog.API.Domain.ValueObjects;
using FluentAssertions;

namespace Catalog.UnitTests.Domain.ValueObjects;

public class ProductDescriptionTests
{
    [Fact]
    public void Create_WithValidDescription_ShouldReturnProductDescription()
    {
        // Arrange
        string description = "Valid Description";

        // Act
        var productDescription = ProductDescription.Create(description);

        // Assert
        productDescription.Should().NotBeNull();
        productDescription.Value.Should().Be(description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyDescription_ShouldThrowDomainException(string emptyDescription)
    {
        // Act
        var action = () => ProductDescription.Create(emptyDescription);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(CatalogErrors.ProductDescription.Empty.Description);
    }

    [Fact]
    public void Create_WithTooLongDescription_ShouldThrowDomainException()
    {
        // Arrange
        string longDescription = new string('a', 501);

        // Act
        var action = () => ProductDescription.Create(longDescription);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(CatalogErrors.ProductDescription.TooLong.Description);
    }
}