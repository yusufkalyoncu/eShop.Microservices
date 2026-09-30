using System.Net;
using System.Net.Http.Json;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.ValueObjects;
using Catalog.API.Features.Products.UpdateProduct;
using Catalog.API.Infrastructure.Data;
using Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Features.Products;

public class UpdateProductTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Update_Product_When_Valid()
    {
        // Arrange
        Guid productId;
        Guid categoryId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var category = Category.Create(CategoryName.Create("TestCat"), CategoryDescription.Create("Desc"));
            categoryId = category.Id;
            dbContext.Categories.Add(category);
            
            var product = Product.Create(
                ProductName.Create("Old Name"),
                ProductDescription.Create("Old Desc"),
                Money.Create(100m, "USD"),
                categoryId
            );
            productId = product.Id;
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();
        }

        var command = new UpdateProductCommand(
            productId,
            "Updated Product Name",
            "Updated Product Desc",
            250.75m,
            "USD",
            categoryId
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/products/{productId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var productInDb = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
            productInDb.Should().NotBeNull();
            productInDb.Name.Value.Should().Be("Updated Product Name");
            productInDb.Description.Value.Should().Be("Updated Product Desc");
            productInDb.Price.Amount.Should().Be(250.75m);
        }
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();
        var command = new UpdateProductCommand(
            nonExistentProductId,
            "Name",
            "Desc",
            10m,
            "USD",
            Guid.NewGuid() // Random category since product doesn't exist anyway
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/products/{nonExistentProductId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
