using System.Net;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.ValueObjects;
using Catalog.API.Infrastructure.Data;
using Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Features.Categories;

public class DeleteCategoryTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Delete_Category_When_Exists()
    {
        // Arrange
        Guid categoryId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var category = Category.Create(
                CategoryName.Create("Delete Me Category"),
                CategoryDescription.Create("This will be deleted")
            );
            categoryId = category.Id;
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.DeleteAsync($"/categories/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var categoryInDb = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            categoryInDb.Should().BeNull();
        }
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        // Arrange
        var nonExistentCategoryId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/categories/{nonExistentCategoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}