using Catalog.API.Domain.Entities;
using Catalog.API.Domain.ValueObjects;
using FluentAssertions;

namespace Catalog.UnitTests.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnProduct()
    {
        // Arrange
        var name = ProductName.Create("iPhone 15");
        var description = ProductDescription.Create("Latest Apple Smartphone");
        var price = Money.Create(49999m, "TRY");
        var categoryId = Guid.NewGuid();

        // Act
        var product = Product.Create(name, description, price, categoryId);

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be(name);
        product.Description.Should().Be(description);
        product.Price.Should().Be(price);
        product.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public void UpdatePrice_WithValidMoney_ShouldUpdatePrice()
    {
        // Arrange
        var product = Product.Create(
            ProductName.Create("iPhone 15"),
            ProductDescription.Create("Description"),
            Money.Create(100m, "TRY"),
            Guid.NewGuid());
        
        var newPrice = Money.Create(150m, "USD");

        // Act
        product.UpdatePrice(newPrice);

        // Assert
        product.Price.Should().Be(newPrice);
    }

    [Fact]
    public void ChangeCategory_WithValidId_ShouldUpdateCategoryId()
    {
        // Arrange
        var product = Product.Create(
            ProductName.Create("iPhone 15"),
            ProductDescription.Create("Description"),
            Money.Create(100m, "TRY"),
            Guid.NewGuid());
        
        var newCategoryId = Guid.NewGuid();

        // Act
        product.ChangeCategory(newCategoryId);

        // Assert
        product.CategoryId.Should().Be(newCategoryId);
    }

    [Fact]
    public void UpdateDetails_WithValidDetails_ShouldUpdateDetails()
    {
        // Arrange
        var product = Product.Create(
            ProductName.Create("Old Name"),
            ProductDescription.Create("Old Description"),
            Money.Create(100m, "TRY"),
            Guid.NewGuid());
        
        var newName = ProductName.Create("New Name");
        var newDescription = ProductDescription.Create("New Description");

        // Act
        product.UpdateDetails(newName, newDescription);

        // Assert
        product.Name.Should().Be(newName);
        product.Description.Should().Be(newDescription);
    }
}