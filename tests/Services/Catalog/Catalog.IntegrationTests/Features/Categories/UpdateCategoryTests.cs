using System.Net;
using System.Net.Http.Json;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.ValueObjects;
using Catalog.API.Features.Categories.UpdateCategory;
using Catalog.API.Infrastructure.Data;
using Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Features.Categories;

public class UpdateCategoryTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Update_Category_When_Valid()
    {
        // Arrange
        Guid categoryId;
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var category = Category.Create(
                CategoryName.Create("Old Name"),
                CategoryDescription.Create("Old Description")
            );
            categoryId = category.Id;
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
        }

        var command = new UpdateCategoryCommand(categoryId, "Updated Name", "Updated Description");

        // Act
        var response = await _client.PutAsJsonAsync($"/categories/{categoryId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var categoryInDb = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            categoryInDb.Should().NotBeNull();
            categoryInDb.Name.Value.Should().Be("Updated Name");
            categoryInDb.Description.Value.Should().Be("Updated Description");
        }
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        // Arrange
        var nonExistentCategoryId = Guid.NewGuid();
        var command = new UpdateCategoryCommand(nonExistentCategoryId, "Name", "Desc");

        // Act
        var response = await _client.PutAsJsonAsync($"/categories/{nonExistentCategoryId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}