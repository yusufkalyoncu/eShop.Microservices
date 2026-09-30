using FluentAssertions;
using Inventory.API.Domain.Entities;
using Inventory.API.Features.Stock.ReserveStock;
using Inventory.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using Microsoft.Data.Sqlite;

namespace Inventory.UnitTests.Features.Stock;

public class ReserveStockHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly InventoryDbContext _dbContext;
    private readonly ReserveStockHandler _handler;

    public ReserveStockHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(_connection)
            .Options;
        _dbContext = new InventoryDbContext(options);
        _dbContext.Database.EnsureCreated();
        
        _handler = new ReserveStockHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenAnyProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new ReserveStockCommand(new Dictionary<Guid, int> { { productId, 5 } });

        // Act
        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<BuildingBlocks.Core.Domain.Exceptions.DomainException>();
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenStockIsInsufficient()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(productId, 5);
        _dbContext.InventoryItems.Add(item);
        await _dbContext.SaveChangesAsync();

        var command = new ReserveStockCommand(new Dictionary<Guid, int> { { productId, 10 } });

        // Act
        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<BuildingBlocks.Core.Domain.Exceptions.DomainException>();

        // Verify state is unchanged
        var dbItem = await _dbContext.InventoryItems.FirstAsync(x => x.ProductId == productId);
        dbItem.AvailableQuantity.Value.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldReserveStock_WhenAllProductsHaveSufficientStock()
    {
        // Arrange
        var product1 = Guid.NewGuid();
        var product2 = Guid.NewGuid();
        
        _dbContext.InventoryItems.Add(InventoryItem.Create(product1, 10));
        _dbContext.InventoryItems.Add(InventoryItem.Create(product2, 20));
        await _dbContext.SaveChangesAsync();

        var command = new ReserveStockCommand(new Dictionary<Guid, int> 
        { 
            { product1, 5 },
            { product2, 10 }
        });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var item1 = await _dbContext.InventoryItems.FirstAsync(x => x.ProductId == product1);
        var item2 = await _dbContext.InventoryItems.FirstAsync(x => x.ProductId == product2);
        
        item1.AvailableQuantity.Value.Should().Be(5);
        item2.AvailableQuantity.Value.Should().Be(10);
    }
}