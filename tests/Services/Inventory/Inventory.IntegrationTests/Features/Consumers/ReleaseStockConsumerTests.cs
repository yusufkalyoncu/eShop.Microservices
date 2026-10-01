using FluentAssertions;
using Inventory.API.Domain.Entities;
using Inventory.API.Infrastructure.Data;
using Inventory.Contracts.IntegrationCommands;
using Inventory.IntegrationTests.Infrastructure;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.IntegrationTests.Features.Consumers;

public class ReleaseStockConsumerTests(InventoryApiFactory factory) : IClassFixture<InventoryApiFactory>
{
    private readonly ITestHarness _testHarness = factory.Services.GetRequiredService<ITestHarness>();

    [Fact]
    public async Task Should_Release_Stock_Successfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var stock = InventoryItem.Create(productId, 5); // 5 available initially
            dbContext.InventoryItems.Add(stock);
            await dbContext.SaveChangesAsync();
        }

        var command = new ReleaseStockCommand
        {
            OrderId = orderId,
            Items = new Dictionary<Guid, int>
            {
                { productId, 5 } // release 5 more
            }
        };

        // Act
        await _testHarness.Bus.Publish(command);

        // Assert
        var consumed = await _testHarness.Consumed.Any<ReleaseStockCommand>();
        consumed.Should().BeTrue();

        await Task.Delay(500);

        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            var stock = await dbContext.InventoryItems.FirstOrDefaultAsync(s => s.ProductId == productId);
            stock.Should().NotBeNull();
            stock.AvailableQuantity.Value.Should().Be(10); // 5 + 5 = 10
        }
    }
}