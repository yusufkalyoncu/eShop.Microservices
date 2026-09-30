using System.Net;
using System.Net.Http.Json;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.ValueObjects;
using Catalog.API.Features.Categories.GetCategories;
using Catalog.API.Infrastructure.Data;
using Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Features.Categories;

public class GetCategoriesTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Return_All_Categories()
    {
        // Arrange
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            
            // Clean up possible existing categories from other tests or just add new ones
            var cat1 = Category.Create(CategoryName.Create("GetCat1"), CategoryDescription.Create("Desc1"));
            var cat2 = Category.Create(CategoryName.Create("GetCat2"), CategoryDescription.Create("Desc2"));
            
            dbContext.Categories.AddRange(cat1, cat2);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        categories.Should().NotBeNull();
        categories.Should().Contain(c => c.Name == "GetCat1");
        categories.Should().Contain(c => c.Name == "GetCat2");
    }
}