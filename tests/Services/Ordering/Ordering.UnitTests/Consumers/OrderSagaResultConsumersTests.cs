using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Consumers;
using Ordering.API.Domain.Enums;
using Ordering.API.Domain.Models;
using Ordering.API.Domain.ValueObjects;
using Ordering.API.Infrastructure.Database;
using Ordering.Contracts.IntegrationCommands;

namespace Ordering.UnitTests.Consumers;

public class OrderSagaResultConsumersTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OrderingDbContext _dbContext;

    public OrderSagaResultConsumersTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseSqlite(_connection)
            .Options;
            
        _dbContext = new OrderingDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    private Order CreateAndSaveOrder()
    {
        var address = new Address("F", "L", "e@m.com", "S", "C", "S", "Z");
        var payment = new PaymentDetails("tok");
        var order = Order.Create(Guid.NewGuid(), "user", address, address, payment);
        
        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();
        return order;
    }

    [Fact]
    public async Task CompleteOrderConsumer_ShouldMarkOrderAsPaid()
    {
        // Arrange
        var order = CreateAndSaveOrder();
        var consumer = new CompleteOrderConsumer(_dbContext);
        var command = new CompleteOrderCommand { OrderId = order.Id };

        // Act
        await consumer.HandleAsync(command, CancellationToken.None);

        // Assert
        var dbOrder = await _dbContext.Orders.FindAsync(order.Id);
        dbOrder!.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public async Task CancelOrderConsumer_ShouldMarkOrderAsCancelled()
    {
        // Arrange
        var order = CreateAndSaveOrder();
        var consumer = new CancelOrderConsumer(_dbContext);
        var command = new CancelOrderCommand { OrderId = order.Id };

        // Act
        await consumer.HandleAsync(command, CancellationToken.None);

        // Assert
        var dbOrder = await _dbContext.Orders.FindAsync(order.Id);
        dbOrder!.Status.Should().Be(OrderStatus.Cancelled);
    }
}