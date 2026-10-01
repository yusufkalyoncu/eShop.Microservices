using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ordering.API.Domain.Models;
using Ordering.API.Domain.ValueObjects;
using Ordering.API.Features.Orders.GetOrderById;
using Ordering.API.Infrastructure.Database;
using Ordering.IntegrationTests.Infrastructure;

namespace Ordering.IntegrationTests.Features.Orders;

public class GetOrderByIdTests(OrderingApiFactory factory) : IClassFixture<OrderingApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<Guid> SeedOrderAsync(string userName)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

        var address = new Address("First", "Last", "email@test.com", "Line 1", "Country", "State", "Zip");
        var payment = new PaymentDetails("tok_visa");
        
        var order = Order.Create(Guid.NewGuid(), userName, address, address, payment);
        order.Add(Guid.NewGuid(), "Test Product", 2, 50.0m);
        
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        return order.Id;
    }

    [Fact]
    public async Task Should_Return_Specific_Order_When_Exists()
    {
        // Arrange
        var userName = "get_order_by_id_user";
        var orderId = await SeedOrderAsync(userName);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/orders/id/{orderId}");
        request.Headers.Add("x-test-user", userName);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<GetOrderByIdResult>();
        result.Should().NotBeNull();
        result.Order.Should().NotBeNull();
        result.Order.Id.Should().Be(orderId);
        result.Order.UserName.Should().Be(userName);
        result.Order.TotalPrice.Should().Be(100.0m);
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Order_Does_Not_Exist()
    {
        // Arrange
        var userName = "get_order_by_id_fail_user";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/orders/id/{Guid.NewGuid()}");
        request.Headers.Add("x-test-user", userName);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}