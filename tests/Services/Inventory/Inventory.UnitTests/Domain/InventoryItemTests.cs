using BuildingBlocks.Core.Domain.Exceptions;
using FluentAssertions;
using Inventory.API.Domain.Entities;

namespace Inventory.UnitTests.Domain;

public class InventoryItemTests
{
    [Fact]
    public void Create_ShouldInitializeItemCorrectly()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var item = InventoryItem.Create(productId, 10);

        // Assert
        item.Id.Should().NotBeEmpty();
        item.ProductId.Should().Be(productId);
        item.AvailableQuantity.Value.Should().Be(10);
    }

    [Fact]
    public void AddStock_ShouldIncreaseAvailableQuantity()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 10);

        // Act
        item.AddStock(5);

        // Assert
        item.AvailableQuantity.Value.Should().Be(15);
    }

    [Fact]
    public void ReserveStock_ShouldDecreaseAvailableQuantity_WhenStockIsSufficient()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 10);

        // Act
        item.ReserveStock(3);

        // Assert
        item.AvailableQuantity.Value.Should().Be(7);
    }

    [Fact]
    public void ReserveStock_ShouldThrowDomainException_WhenStockIsInsufficient()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 5);

        // Act
        Action action = () => item.ReserveStock(10);

        // Assert
        action.Should().Throw<DomainException>()
            .WithMessage("*Insufficient stock*");
    }

    [Fact]
    public void ReleaseStock_ShouldIncreaseAvailableQuantity()
    {
        // Arrange
        var item = InventoryItem.Create(Guid.NewGuid(), 10);

        // Act
        item.ReleaseStock(5);

        // Assert
        item.AvailableQuantity.Value.Should().Be(15);
    }
}