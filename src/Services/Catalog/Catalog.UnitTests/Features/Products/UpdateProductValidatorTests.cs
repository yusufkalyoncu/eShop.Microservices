using Catalog.API.Features.Products.UpdateProduct;
using FluentAssertions;

namespace Catalog.UnitTests.Features.Products;

public class UpdateProductValidatorTests
{
    private readonly UpdateProductValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var command = new UpdateProductCommand(
            Id: Guid.NewGuid(),
            Name: "iPhone 15",
            Description: "New Apple Smartphone",
            Price: 49999m,
            Currency: "TRY",
            CategoryId: Guid.NewGuid()
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
        var command = new UpdateProductCommand(
            Id: Guid.Empty,
            Name: "Valid Name",
            Description: "Description",
            Price: 100,
            Currency: "TRY",
            CategoryId: Guid.NewGuid()
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }
}