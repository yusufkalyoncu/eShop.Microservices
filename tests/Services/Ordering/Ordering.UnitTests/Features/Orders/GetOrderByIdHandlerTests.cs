using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Domain.Models;
using Ordering.API.Domain.ValueObjects;
using Ordering.API.Features.Orders.GetOrderById;
using Ordering.API.Infrastructure.Database;

namespace Ordering.UnitTests.Features.Orders;

public class GetOrderByIdHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OrderingDbContext _dbContext;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseSqlite(_connection)
            .Options;
            
        _dbContext = new OrderingDbContext(options);
        _dbContext.Database.EnsureCreated();

        _handler = new GetOrderByIdQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldReturnOrder_WhenExists()
    {
        // Arrange
        var address = new Address("F", "L", "e@m.com", "S", "C", "S", "Z");
        var payment = new PaymentDetails("tok");
        var order = Order.Create(Guid.NewGuid(), "testuser", address, address, payment);
        
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        var query = new GetOrderByIdQuery(order.Id, "testuser");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Order.Id.Should().Be(order.Id);
        result.Data.Order.UserName.Should().Be("testuser");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        // Arrange
        var query = new GetOrderByIdQuery(Guid.NewGuid(), "testuser");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Order.NotFound");
    }
}