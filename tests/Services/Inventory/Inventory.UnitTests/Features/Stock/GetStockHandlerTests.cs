using FluentAssertions;
using Inventory.API.Domain.Entities;
using Inventory.API.Features.Stock.GetStock;
using Inventory.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using Microsoft.Data.Sqlite;

namespace Inventory.UnitTests.Features.Stock;

public class GetStockHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly InventoryDbContext _dbContext;
    private readonly GetStockHandler _handler;

    public GetStockHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(_connection)
            .Options;
        _dbContext = new InventoryDbContext(options);
        _dbContext.Database.EnsureCreated();
        
        _handler = new GetStockHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldReturnZero_WhenProductDoesNotExist()
    {
        // Arrange
        var query = new GetStockQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AvailableQuantity.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnAvailableQuantity_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var item = InventoryItem.Create(productId, 25);
        _dbContext.InventoryItems.Add(item);
        await _dbContext.SaveChangesAsync();

        var query = new GetStockQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AvailableQuantity.Should().Be(25);
    }
}