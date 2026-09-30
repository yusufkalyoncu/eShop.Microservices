using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Core.Pagination;
using Catalog.API.Domain.Entities;
using Catalog.API.Domain.ValueObjects;
using Catalog.API.Features.Products.GetProducts;
using Catalog.API.Infrastructure.Data;
using Catalog.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.IntegrationTests.Features.Products;

public class GetProductsTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Return_Paginated_Products()
    {
        // Arrange
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var category = Category.Create(CategoryName.Create("PaginatedCat"), CategoryDescription.Create("Desc"));
            dbContext.Categories.Add(category);

            // Add 3 products
            dbContext.Products.Add(Product.Create(ProductName.Create("Prod1"), ProductDescription.Create("D1"), Money.Create(10m), category.Id));
            dbContext.Products.Add(Product.Create(ProductName.Create("Prod2"), ProductDescription.Create("D2"), Money.Create(20m), category.Id));
            dbContext.Products.Add(Product.Create(ProductName.Create("Prod3"), ProductDescription.Create("D3"), Money.Create(30m), category.Id));
            
            await dbContext.SaveChangesAsync();
        }

        // Act - Request Page 1 with Size 2
        var response = await _client.GetAsync("/products?PageIndex=1&PageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var paginatedResult = await response.Content.ReadFromJsonAsync<PaginatedResult<ProductDto>>();
        paginatedResult.Should().NotBeNull();
        paginatedResult.PageIndex.Should().Be(1);
        paginatedResult.PageSize.Should().Be(2);
        paginatedResult.Data.Should().NotBeNull();
        paginatedResult.Data.Should().HaveCountLessThanOrEqualTo(2);
    }
}