using FluentAssertions;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ordering.API.Domain.Models;
using Ordering.API.Domain.ValueObjects;
using Ordering.API.Infrastructure.Database;
using Ordering.Contracts.IntegrationCommands;
using Ordering.IntegrationTests.Infrastructure;

namespace Ordering.IntegrationTests.Features.Consumers;

public class OrderSagaResultConsumerTests(OrderingApiFactory factory) : IClassFixture<OrderingApiFactory>
{
    private readonly ITestHarness _testHarness = factory.Services.GetRequiredService<ITestHarness>();

    private async Task<Guid> SeedOrderAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();

        var address = new Address("First", "Last", "email@test.com", "Line 1", "Country", "State", "Zip");
        var payment = new PaymentDetails("tok_visa");
        
        var orderId = Guid.NewGuid();
        var order = Order.Create(orderId, "saga_test_user", address, address, payment);
        
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();

        return order.Id;
    }

    [Fact]
    public async Task Should_Consume_CompleteOrderCommand_And_Mark_As_Paid()
    {
        // Arrange
        var orderId = await SeedOrderAsync();
        var command = new CompleteOrderCommand { OrderId = orderId };

        // Act
        await _testHarness.Bus.Publish(command);

        // Assert
        var consumed = await _testHarness.Consumed.Any<CompleteOrderCommand>();
        consumed.Should().BeTrue();

        await Task.Delay(500);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        order.Should().NotBeNull();
        order.Status.ToString().Should().Be("Paid");
    }

    [Fact]
    public async Task Should_Consume_CancelOrderCommand_And_Mark_As_Cancelled()
    {
        // Arrange
        var orderId = await SeedOrderAsync();
        var command = new CancelOrderCommand { OrderId = orderId };

        // Act
        await _testHarness.Bus.Publish(command);

        // Assert
        var consumed = await _testHarness.Consumed.Any<CancelOrderCommand>();
        consumed.Should().BeTrue();

        await Task.Delay(500);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        order.Should().NotBeNull();
        order.Status.ToString().Should().Be("Cancelled");
    }
}