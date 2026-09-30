using System.Net;
using System.Net.Http.Json;
using Catalog.API.Features.Products.CreateProduct;
using Catalog.API.Infrastructure.Data;
using Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Features;

public class CreateProductTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Create_Product_When_Request_Is_Valid()
    {
        Guid categoryId;
        
        using (var arrangeScope = factory.Services.CreateScope())
        {
            var arrangeDbContext = arrangeScope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var category = API.Domain.Entities.Category.Create(
                API.Domain.ValueObjects.CategoryName.Create("TestCategory"),
                API.Domain.ValueObjects.CategoryDescription.Create("Test Category Description")
            );
            categoryId = category.Id;
            arrangeDbContext.Categories.Add(category);
            await arrangeDbContext.SaveChangesAsync();
        }

        var command = new CreateProductCommand(
            "Test Product",
            "This is a test product description",
            99.99m,
            categoryId
        );

        // Act
        // Note: The HTTP client will automatically use "TestAuthHandler" bypassing Keycloak.
        var response = await _client.PostAsJsonAsync("/products", command);

        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because: " + content);
        
        var result = await response.Content.ReadFromJsonAsync<CreateProductResponse>();
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        // Assert 2: Database state
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        
        var productInDb = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == result.Id);
        
        productInDb.Should().NotBeNull();
        productInDb.Name.Value.Should().Be("Test Product");
        productInDb.Price.Amount.Should().Be(99.99m);
        productInDb.CategoryId.Should().Be(categoryId);
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        // Arrange
        var nonExistentCategoryId = Guid.NewGuid();
        var command = new CreateProductCommand(
            "Test Product",
            "This is a test product description",
            99.99m,
            nonExistentCategoryId
        );

        // Act
        var response = await _client.PostAsJsonAsync("/products", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}