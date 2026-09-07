using BuildingBlocks.Core.Domain.Exceptions;
using Catalog.API.Domain.Errors;
using Catalog.API.Domain.ValueObjects;
using FluentAssertions;

namespace Catalog.UnitTests.Domain.ValueObjects;

public class CategoryNameTests
{
    [Fact]
    public void Create_WithValidName_ShouldReturnCategoryName()
    {
        // Arrange
        string name = "Electronics";

        // Act
        var categoryName = CategoryName.Create(name);

        // Assert
        categoryName.Should().NotBeNull();
        categoryName.Value.Should().Be(name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldThrowDomainException(string emptyName)
    {
        // Act
        var action = () => CategoryName.Create(emptyName);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(CatalogErrors.CategoryName.Empty.Description);
    }

    [Fact]
    public void Create_WithTooShortName_ShouldThrowDomainException()
    {
        // Arrange
        string shortName = "ab";

        // Act
        var action = () => CategoryName.Create(shortName);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(CatalogErrors.CategoryName.TooShort.Description);
    }

    [Fact]
    public void Create_WithTooLongName_ShouldThrowDomainException()
    {
        // Arrange
        string longName = new string('a', 51);

        // Act
        var action = () => CategoryName.Create(longName);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(CatalogErrors.CategoryName.TooLong.Description);
    }
}