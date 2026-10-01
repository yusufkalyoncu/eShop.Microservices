using FluentAssertions;
using Inventory.API.Domain.Entities;
using Inventory.API.Infrastructure.Data;
using Inventory.Contracts.IntegrationCommands;
using Inventory.Contracts.IntegrationEvents;
using Inventory.IntegrationTests.Infrastructure;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.IntegrationTests.Features.Consumers;

public class ReserveStockConsumerTests(InventoryApiFactory factory) : IClassFixture<InventoryApiFactory>
{
    private readonly ITestHarness _testHarness = factory.Services.GetRequiredService<ITestHarness>();

    [Fact]
    public async Task Should_Reserve_Stock_When_Available()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var stock = InventoryItem.Create(productId, 10); // 10 available
            dbContext.InventoryItems.Add(stock);
            await dbContext.SaveChangesAsync();
        }

        var command = new ReserveStockCommand
        {
            OrderId = orderId,
            Items = new Dictionary<Guid, int>
            {
                { productId, 5 } // reserve 5
            }
        };

        // Act
        await _testHarness.Bus.Publish(command);

        // Assert
        var consumed = await _testHarness.Consumed.Any<ReserveStockCommand>();
        consumed.Should().BeTrue();

        await Task.Delay(500);

        var published = await _testHarness.Published.Any<StockReservedEvent>(e => e.Context != null && e.Context.Message.OrderId == orderId);
        published.Should().BeTrue();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var stock = await dbContext.InventoryItems.FirstOrDefaultAsync(s => s.ProductId == productId);
            stock.Should().NotBeNull();
            stock.AvailableQuantity.Value.Should().Be(5);
        }
    }

    [Fact]
    public async Task Should_Fail_Reservation_When_Not_Enough_Stock()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var stock = InventoryItem.Create(productId, 2); // 2 available
            dbContext.InventoryItems.Add(stock);
            await dbContext.SaveChangesAsync();
        }

        var command = new ReserveStockCommand
        {
            OrderId = orderId,
            Items = new Dictionary<Guid, int>
            {
                { productId, 5 } // reserve 5 (should fail)
            }
        };

        // Act
        await _testHarness.Bus.Publish(command);

        // Assert
        var consumed = await _testHarness.Consumed.Any<ReserveStockCommand>();
        consumed.Should().BeTrue();

        await Task.Delay(500);

        var published = await _testHarness.Published.Any<StockReservationFailedEvent>(e => e.Context != null && e.Context.Message.OrderId == orderId);
        published.Should().BeTrue();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var stock = await dbContext.InventoryItems.FirstOrDefaultAsync(s => s.ProductId == productId);
            stock.Should().NotBeNull();
            stock.AvailableQuantity.Value.Should().Be(2); // unchanged
        }
    }
}