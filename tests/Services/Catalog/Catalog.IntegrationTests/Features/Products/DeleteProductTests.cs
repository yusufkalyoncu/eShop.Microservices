using System.Net;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.ValueObjects;
using Catalog.API.Infrastructure.Data;
using Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Features.Products;

public class DeleteProductTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Delete_Product_When_Exists()
    {
        // Arrange
        Guid productId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            
            // Need a category first
            var category = Category.Create(CategoryName.Create("TestCat"), CategoryDescription.Create("Desc"));
            dbContext.Categories.Add(category);
            
            var product = Product.Create(
                ProductName.Create("Product To Delete"),
                ProductDescription.Create("Will be deleted"),
                Money.Create(10m),
                category.Id
            );
            productId = product.Id;
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.DeleteAsync($"/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var productInDb = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId);
            productInDb.Should().BeNull();
        }
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/products/{nonExistentProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}