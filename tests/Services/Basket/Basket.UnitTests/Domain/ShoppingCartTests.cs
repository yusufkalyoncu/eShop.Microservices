using Basket.API.Domain.Models;
using FluentAssertions;

namespace Basket.UnitTests.Domain;

public class ShoppingCartTests
{
    [Fact]
    public void Constructor_ShouldSetUserNameAndId()
    {
        // Arrange
        const string userName = "testuser";

        // Act
        var cart = new ShoppingCart(userName);

        // Assert
        cart.UserName.Should().Be(userName);
        cart.Id.Should().Be(userName);
        cart.Items.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void AddItem_ShouldAddNewItem_WhenItemDoesNotExist()
    {
        // Arrange
        var cart = new ShoppingCart("testuser");
        var productId = Guid.NewGuid();
        var productName = "Test Product";
        var quantity = 2;
        var price = 50.0m;

        // Act
        cart.AddItem(productId, productName, quantity, price);

        // Assert
        cart.Items.Should().HaveCount(1);
        var addedItem = cart.Items.First();
        addedItem.ProductId.Should().Be(productId);
        addedItem.ProductName.Should().Be(productName);
        addedItem.Quantity.Should().Be(quantity);
        addedItem.Price.Should().Be(price);
    }

    [Fact]
    public void AddItem_ShouldIncreaseQuantity_WhenItemAlreadyExists()
    {
        // Arrange
        var cart = new ShoppingCart("testuser");
        var productId = Guid.NewGuid();
        cart.AddItem(productId, "Test Product", 2, 50.0m);

        // Act
        // Add 3 more of the same product
        cart.AddItem(productId, "Test Product", 3, 50.0m);

        // Assert
        cart.Items.Should().HaveCount(1); // Should still be 1 item
        cart.Items.First().Quantity.Should().Be(5); // 2 + 3 = 5
    }

    [Fact]
    public void TotalPrice_ShouldCalculateCorrectly_BasedOnItems()
    {
        // Arrange
        var cart = new ShoppingCart("testuser");
        
        // Item 1: 2 * 50 = 100
        cart.AddItem(Guid.NewGuid(), "Product A", 2, 50.0m);
        // Item 2: 1 * 150 = 150
        cart.AddItem(Guid.NewGuid(), "Product B", 1, 150.0m);
        // Item 3: 3 * 20 = 60
        cart.AddItem(Guid.NewGuid(), "Product C", 3, 20.0m);

        // Act
        var totalPrice = cart.TotalPrice;

        // Assert
        totalPrice.Should().Be(310.0m); // 100 + 150 + 60
    }
}