using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Inventory.API.Features.Stock.AddStock;
using Inventory.IntegrationTests.Infrastructure;

namespace Inventory.IntegrationTests.Features.Endpoints;

public class AddStockTests(InventoryApiFactory factory) : IClassFixture<InventoryApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Add_Stock_Successfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new AddStockRequest(productId, 100);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/stock/add")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("x-test-user", "test_admin");

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify with GET
        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/stock/{productId}");
        getRequest.Headers.Add("x-test-user", "test_admin");
        
        var getResponse = await _client.SendAsync(getRequest);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Wait, GetStockEndpoint returns GetStockResponse record. Wait, we don't need to deserialize the exact record if it's just json, 
        // but we can parse it as dynamic or just check string. Or redefine a private record.
        var jsonResponse = await getResponse.Content.ReadFromJsonAsync<System.Text.Json.Nodes.JsonObject>();
        jsonResponse.Should().NotBeNull();
        
        var availableQuantity = jsonResponse["availableQuantity"]?.GetValue<int>();
        availableQuantity.Should().Be(100);
    }
}