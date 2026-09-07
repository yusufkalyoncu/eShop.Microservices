using Catalog.API.Features.Categories.UpdateCategory;
using FluentAssertions;

namespace Catalog.UnitTests.Features.Categories;

public class UpdateCategoryValidatorTests
{
    private readonly UpdateCategoryValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Id: Guid.NewGuid(),
            Name: "Electronics",
            Description: "Electronic devices and gadgets"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Id: Guid.Empty,
            Name: "Electronics",
            Description: "Description"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }
}