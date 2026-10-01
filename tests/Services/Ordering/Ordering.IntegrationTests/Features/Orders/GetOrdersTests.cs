using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ordering.API.Domain.Models;
using Ordering.API.Domain.ValueObjects;
using Ordering.API.Features.Orders.GetOrders;
using Ordering.API.Infrastructure.Database;
using Ordering.IntegrationTests.Infrastructure;

namespace Ordering.IntegrationTests.Features.Orders;

public class GetOrdersTests(OrderingApiFactory factory) : IClassFixture<OrderingApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task SeedOrdersAsync(string userName)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

        var address = new Address("First", "Last", "email@test.com", "Line 1", "Country", "State", "Zip");
        var payment = new PaymentDetails("tok_visa");
        
        for (int i = 0; i < 3; i++)
        {
            var order = Order.Create(Guid.NewGuid(), userName, address, address, payment);
            order.Add(Guid.NewGuid(), $"Product {i}", 1, 10.0m * (i + 1));
            dbContext.Orders.Add(order);
        }
        
        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Should_Return_Orders_For_User()
    {
        // Arrange
        var userName = "get_orders_test_user";
        await SeedOrdersAsync(userName);

        var request = new HttpRequestMessage(HttpMethod.Get, "/orders");
        request.Headers.Add("x-test-user", userName);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<GetOrdersResult>();
        result.Should().NotBeNull();
        result.Orders.Should().HaveCountGreaterThanOrEqualTo(3);
        result.Orders.Should().OnlyContain(o => o.UserName == userName);
    }
}