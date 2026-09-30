using Basket.Contracts.IntegrationEvents;
using BuildingBlocks.Messaging.Abstractions;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Ordering.API.Consumers;
using Ordering.API.Infrastructure.Database;
using Ordering.Contracts.IntegrationEvents;

namespace Ordering.UnitTests.Consumers;

public class BasketCheckoutIntegrationEventHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OrderingDbContext _dbContext;
    private readonly IEventBus _eventBusMock;
    private readonly BasketCheckoutIntegrationEventHandler _handler;

    public BasketCheckoutIntegrationEventHandlerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseSqlite(_connection)
            .Options;
            
        _dbContext = new OrderingDbContext(options);
        _dbContext.Database.EnsureCreated();

        _eventBusMock = Substitute.For<IEventBus>();
        _handler = new BasketCheckoutIntegrationEventHandler(_dbContext, _eventBusMock);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateOrderAndPublishEvent()
    {
        // Arrange
        var @event = new BasketCheckoutIntegrationEvent
        {
            UserName = "johndoe",
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "john@doe.com",
            AddressLine = "123 Main St",
            Country = "USA",
            State = "NY",
            ZipCode = "10001",
            PaymentToken = "tok_123",
            Items =
            [
                new BasketItemDto
                    { ProductId = Guid.NewGuid(), ProductName = "Product A", Quantity = 2, Price = 50.0m },
                new BasketItemDto
                    { ProductId = Guid.NewGuid(), ProductName = "Product B", Quantity = 1, Price = 100.0m }
            ]
        };

        // Act
        await _handler.HandleAsync(@event, CancellationToken.None);

        // Assert
        var order = await _dbContext.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(x => x.UserName == "johndoe");
        order.Should().NotBeNull();
        order.OrderItems.Should().HaveCount(2);
        order.TotalPrice.Should().Be(200.0m);
        order.Payment.PaymentToken.Should().Be("tok_123");

        await _eventBusMock.Received(1).PublishAsync(
            Arg.Is<OrderPlacedIntegrationEvent>(e => 
                e.OrderId == order.Id &&
                e.UserName == "johndoe" &&
                e.TotalAmount == 200.0m &&
                e.Items.Count == 2),
            Arg.Any<CancellationToken>());
    }
}