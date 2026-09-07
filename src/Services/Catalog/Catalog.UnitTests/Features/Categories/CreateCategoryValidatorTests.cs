using Catalog.API.Features.Categories.CreateCategory;
using FluentAssertions;

namespace Catalog.UnitTests.Features.Categories;

public class CreateCategoryValidatorTests
{
    private readonly CreateCategoryValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var command = new CreateCategoryCommand(
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
    public void Validate_WithEmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            Name: "",
            Description: "Description"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }
}