using FluentAssertions;
using Inventory.API.Features.Stock.AddStock;
using Inventory.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using Microsoft.Data.Sqlite;

namespace Inventory.UnitTests.Features.Stock;

public class AddStockHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly InventoryDbContext _dbContext;
    private readonly AddStockHandler _handler;

    public AddStockHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(_connection)
            .Options;
        _dbContext = new InventoryDbContext(options);
        _dbContext.Database.EnsureCreated();
        
        _handler = new AddStockHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldCreateNewItem_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new AddStockCommand(productId, 10);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var item = await _dbContext.InventoryItems.FirstOrDefaultAsync(x => x.ProductId == productId);
        item.Should().NotBeNull();
        item!.AvailableQuantity.Value.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldIncreaseStock_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var existingCommand = new AddStockCommand(productId, 5);
        await _handler.Handle(existingCommand, CancellationToken.None); // initial add
        
        var command = new AddStockCommand(productId, 15);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var item = await _dbContext.InventoryItems.FirstOrDefaultAsync(x => x.ProductId == productId);
        item.Should().NotBeNull();
        item!.AvailableQuantity.Value.Should().Be(20);
    }
}