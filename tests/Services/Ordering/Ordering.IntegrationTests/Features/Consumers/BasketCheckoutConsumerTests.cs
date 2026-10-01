using Basket.Contracts.IntegrationEvents;
using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Ordering.API.Infrastructure.Database;
using Ordering.Contracts.IntegrationEvents;
using Ordering.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ordering.IntegrationTests.Features.Consumers;

public class BasketCheckoutConsumerTests(OrderingApiFactory factory) : IClassFixture<OrderingApiFactory>
{
    private readonly ITestHarness _testHarness = factory.Services.GetRequiredService<ITestHarness>();

    [Fact]
    public async Task Should_Consume_CheckoutEvent_And_Create_Order()
    {
        // Arrange
        var @event = new BasketCheckoutIntegrationEvent
        {
            UserName = "checkout_test_user",
            TotalPrice = 100,
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "john@example.com",
            AddressLine = "123 Main St",
            Country = "USA",
            State = "NY",
            ZipCode = "10001",
            PaymentToken = "tok_visa",
            Items = [new BasketItemDto { ProductId = Guid.NewGuid(), ProductName = "Test Product", Price = 100, Quantity = 1 }]
        };

        // Act
        await _testHarness.Bus.Publish(@event);

        // Assert Consumer Processed Event
        var consumed = await _testHarness.Consumed.Any<BasketCheckoutIntegrationEvent>();
        consumed.Should().BeTrue();

        // Give it a brief moment to process the DB transaction and outbox
        await Task.Delay(500);

        // Verify Order is saved in Database
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.UserName == "checkout_test_user");
        order.Should().NotBeNull();
        order.TotalPrice.Should().Be(100);
        order.Status.ToString().Should().Be("Pending");

        // Verify OrderPlacedIntegrationEvent is published
        var published = await _testHarness.Published.Any<OrderPlacedIntegrationEvent>();
        published.Should().BeTrue();
    }
}