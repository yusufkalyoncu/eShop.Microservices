using Catalog.API.Domain.Entities;
using Catalog.API.Domain.ValueObjects;
using FluentAssertions;

namespace Catalog.UnitTests.Domain.Entities;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnCategory()
    {
        // Arrange
        var name = CategoryName.Create("Electronics");
        var description = CategoryDescription.Create("Electronic devices and gadgets");

        // Act
        var category = Category.Create(name, description);

        // Assert
        category.Should().NotBeNull();
        category.Id.Should().NotBeEmpty();
        category.Name.Should().Be(name);
        category.Description.Should().Be(description);
    }

    [Fact]
    public void Update_WithValidParameters_ShouldUpdateCategory()
    {
        // Arrange
        var category = Category.Create(
            CategoryName.Create("Old Name"),
            CategoryDescription.Create("Old Description"));
        
        var newName = CategoryName.Create("New Name");
        var newDescription = CategoryDescription.Create("New Description");

        // Act
        category.Update(newName, newDescription);

        // Assert
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);
    }
}