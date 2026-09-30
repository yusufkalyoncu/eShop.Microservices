using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Domain.Models;
using Ordering.API.Domain.ValueObjects;
using Ordering.API.Features.Orders.GetOrders;
using Ordering.API.Infrastructure.Database;

namespace Ordering.UnitTests.Features.Orders;

public class GetOrdersHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OrderingDbContext _dbContext;
    private readonly GetOrdersQueryHandler _handler;

    public GetOrdersHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseSqlite(_connection)
            .Options;
            
        _dbContext = new OrderingDbContext(options);
        _dbContext.Database.EnsureCreated();

        _handler = new GetOrdersQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldReturnOrdersForUser()
    {
        // Arrange
        var address = new Address("F", "L", "e@m.com", "S", "C", "S", "Z");
        var payment = new PaymentDetails("tok");
        
        for (int i = 0; i < 5; i++)
        {
            var order = Order.Create(Guid.NewGuid(), "target_user", address, address, payment);
            _dbContext.Orders.Add(order);
        }
        for (int i = 0; i < 3; i++)
        {
            var order = Order.Create(Guid.NewGuid(), "other_user", address, address, payment);
            _dbContext.Orders.Add(order);
        }
        await _dbContext.SaveChangesAsync();

        var query = new GetOrdersQuery("target_user");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Orders.Should().HaveCount(5);
        result.Data.Orders.All(o => o.UserName == "target_user").Should().BeTrue();
    }
}