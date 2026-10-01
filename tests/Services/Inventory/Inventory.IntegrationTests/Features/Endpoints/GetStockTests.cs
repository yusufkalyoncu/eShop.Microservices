using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Inventory.API.Domain.Entities;
using Inventory.API.Infrastructure.Data;
using Inventory.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.IntegrationTests.Features.Endpoints;

public class GetStockTests(InventoryApiFactory factory) : IClassFixture<InventoryApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Return_Zero_For_Unknown_Product()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/stock/{productId}");
        request.Headers.Add("x-test-user", "test_user");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var jsonResponse = await response.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonObject>();
        jsonResponse.Should().NotBeNull();
        
        var availableQuantity = jsonResponse["availableQuantity"]?.GetValue<int>();
        availableQuantity.Should().Be(0);
    }

    [Fact]
    public async Task Should_Return_Available_Stock_For_Existing_Product()
    {
        // Arrange
        var productId = Guid.NewGuid();
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var stock = InventoryItem.Create(productId, 50);
            dbContext.InventoryItems.Add(stock);
            await dbContext.SaveChangesAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/stock/{productId}");
        request.Headers.Add("x-test-user", "test_user");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var jsonResponse = await response.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonObject>();
        jsonResponse.Should().NotBeNull();
        
        var availableQuantity = jsonResponse["availableQuantity"]?.GetValue<int>();
        availableQuantity.Should().Be(50);
    }
}