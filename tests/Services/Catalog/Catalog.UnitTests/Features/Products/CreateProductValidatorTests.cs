using Catalog.API.Features.Products.CreateProduct;
using FluentAssertions;

namespace Catalog.UnitTests.Features.Products;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "iPhone 15",
            Description: "New Apple Smartphone",
            Price: 49999m,
            CategoryId: Guid.NewGuid()
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
        var command = new CreateProductCommand(
            Name: "",
            Description: "Description",
            Price: 100,
            CategoryId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithNegativePrice_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateProductCommand(
            Name: "Valid Name",
            Description: "Description",
            Price: -50,
            CategoryId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }
}