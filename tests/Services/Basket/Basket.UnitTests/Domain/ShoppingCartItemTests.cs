using Basket.API.Domain.Models;
using FluentAssertions;

namespace Basket.UnitTests.Domain;

public class ShoppingCartItemTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties_WhenValidArgumentsProvided()
    {
        // Arrange
        var productId = Guid.NewGuid();
        const string productName = "Test Product";
        const int quantity = 2;
        const decimal price = 50.0m;

        // Act
        var item = new ShoppingCartItem(productId, productName, quantity, price);

        // Assert
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be(productName);
        item.Quantity.Should().Be(quantity);
        item.Price.Should().Be(price);
        item.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void UpdateQuantity_ShouldUpdateQuantity_WhenCalled()
    {
        // Arrange
        var item = new ShoppingCartItem(Guid.NewGuid(), "Test Product", 1, 10.0m);
        var newQuantity = 5;

        // Act
        item.UpdateQuantity(newQuantity);

        // Assert
        item.Quantity.Should().Be(newQuantity);
    }

    [Fact]
    public void UpdatePrice_ShouldUpdatePrice_WhenCalled()
    {
        // Arrange
        var item = new ShoppingCartItem(Guid.NewGuid(), "Test Product", 1, 10.0m);
        var newPrice = 25.0m;

        // Act
        item.UpdatePrice(newPrice);

        // Assert
        item.Price.Should().Be(newPrice);
    }
}