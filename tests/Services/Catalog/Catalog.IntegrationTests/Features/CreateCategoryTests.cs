using System.Net;
using System.Net.Http.Json;
using Catalog.API.Features.Categories.CreateCategory;
using Catalog.API.Infrastructure.Data;
using Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Features;

public class CreateCategoryTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Create_Category_When_Request_Is_Valid()
    {
        // Arrange
        var command = new CreateCategoryCommand(
            "Electronics",
            "Electronic devices and accessories"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/categories", command);

        // Assert 1: HTTP Response
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, "because: " + content);
        
        var result = await response.Content.ReadFromJsonAsync<CreateCategoryResponse>();
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        // Assert 2: Database state
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        
        var categoryInDb = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == result.Id);
        
        categoryInDb.Should().NotBeNull();
        categoryInDb.Name.Value.Should().Be("Electronics");
        categoryInDb.Description.Value.Should().Be("Electronic devices and accessories");
    }
}