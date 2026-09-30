using FluentAssertions;
using Inventory.API.Domain.Entities;
using Inventory.API.Features.Stock.ReleaseStock;
using Inventory.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using Microsoft.Data.Sqlite;

namespace Inventory.UnitTests.Features.Stock;

public class ReleaseStockHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly InventoryDbContext _dbContext;
    private readonly ReleaseStockHandler _handler;

    public ReleaseStockHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(_connection)
            .Options;
        _dbContext = new InventoryDbContext(options);
        _dbContext.Database.EnsureCreated();
        
        _handler = new ReleaseStockHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new ReleaseStockCommand(new Dictionary<Guid, int> { { productId, 10 } });

        // Act
        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<BuildingBlocks.Core.Domain.Exceptions.DomainException>();
    }

    [Fact]
    public async Task Handle_ShouldIncreaseStock_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(productId, 5);
        _dbContext.InventoryItems.Add(item);
        await _dbContext.SaveChangesAsync();

        var command = new ReleaseStockCommand(new Dictionary<Guid, int> { { productId, 15 } });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var dbItem = await _dbContext.InventoryItems.FirstOrDefaultAsync(x => x.ProductId == productId);
        dbItem.Should().NotBeNull();
        dbItem!.AvailableQuantity.Value.Should().Be(20);
    }
}