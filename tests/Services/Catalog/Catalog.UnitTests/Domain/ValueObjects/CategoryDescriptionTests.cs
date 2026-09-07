using BuildingBlocks.Core.Domain.Exceptions;
using Catalog.API.Domain.Errors;
using Catalog.API.Domain.ValueObjects;
using FluentAssertions;

namespace Catalog.UnitTests.Domain.ValueObjects;

public class CategoryDescriptionTests
{
    [Fact]
    public void Create_WithValidDescription_ShouldReturnCategoryDescription()
    {
        // Arrange
        string description = "Valid Description";

        // Act
        var categoryDescription = CategoryDescription.Create(description);

        // Assert
        categoryDescription.Should().NotBeNull();
        categoryDescription.Value.Should().Be(description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyDescription_ShouldThrowDomainException(string emptyDescription)
    {
        // Act
        var action = () => CategoryDescription.Create(emptyDescription);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(CatalogErrors.CategoryDescription.Empty.Description);
    }

    [Fact]
    public void Create_WithTooLongDescription_ShouldThrowDomainException()
    {
        // Arrange
        string longDescription = new string('a', 501);

        // Act
        var action = () => CategoryDescription.Create(longDescription);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage(CatalogErrors.CategoryDescription.TooLong.Description);
    }
}