using BuildingBlocks.Core.Domain.Exceptions;
using FluentAssertions;
using Inventory.API.Domain.ValueObjects;

namespace Inventory.UnitTests.Domain;

public class QuantityTests
{
    [Fact]
    public void Create_ShouldReturnQuantity_WhenValueIsZeroOrPositive()
    {
        // Arrange & Act
        var q1 = Quantity.Create(0);
        var q2 = Quantity.Create(10);

        // Assert
        q1.Value.Should().Be(0);
        q2.Value.Should().Be(10);
    }

    [Fact]
    public void Create_ShouldThrowDomainException_WhenValueIsNegative()
    {
        // Arrange
        Action action = () => Quantity.Create(-5);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("*Must be non-negative*");
    }

    [Fact]
    public void Add_ShouldReturnNewQuantityWithIncreasedValue()
    {
        // Arrange
        var q = Quantity.Create(10);

        // Act
        var result = q.Add(5);

        // Assert
        result.Value.Should().Be(15);
        q.Value.Should().Be(10); // Original is unchanged
    }

    [Fact]
    public void Subtract_ShouldReturnNewQuantityWithDecreasedValue()
    {
        // Arrange
        var q = Quantity.Create(10);

        // Act
        var result = q.Subtract(3);

        // Assert
        result.Value.Should().Be(7);
        q.Value.Should().Be(10);
    }

    [Fact]
    public void Subtract_ShouldThrowDomainException_WhenResultWouldBeNegative()
    {
        // Arrange
        var q = Quantity.Create(5);

        // Act
        Action action = () => q.Subtract(10);

        // Assert
        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void ImplicitOperator_ShouldReturnIntValue()
    {
        // Arrange
        var q = Quantity.Create(42);

        // Act
        int value = q;

        // Assert
        value.Should().Be(42);
    }
}