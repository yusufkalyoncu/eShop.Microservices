using System.Net;
using System.Net.Http.Json;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.ValueObjects;
using Catalog.API.Features.Products.GetProducts;
using Catalog.API.Infrastructure.Data;
using Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Features.Products;

public class GetProductByIdTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Return_Product_When_Exists()
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
                ProductName.Create("Get Me Product"),
                ProductDescription.Create("Test Desc"),
                Money.Create(150.5m),
                category.Id
            );
            productId = product.Id;
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var productDto = await response.Content.ReadFromJsonAsync<ProductDto>();
        productDto.Should().NotBeNull();
        productDto.Id.Should().Be(productId);
        productDto.Name.Should().Be("Get Me Product");
        productDto.PriceAmount.Should().Be(150.5m);
        productDto.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/products/{nonExistentProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}