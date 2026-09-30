using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using BuildingBlocks.Messaging.Abstractions;
using Inventory.API.Consumers;
using Inventory.API.Features.Stock.ReserveStock;
using Inventory.Contracts.IntegrationEvents;
using NSubstitute;

namespace Inventory.UnitTests.Consumers;

public class ReserveStockConsumerTests
{
    private readonly ICommandHandler<ReserveStockCommand> _handlerMock;
    private readonly IEventBus _eventBusMock;
    private readonly ReserveStockConsumer _consumer;

    public ReserveStockConsumerTests()
    {
        _handlerMock = Substitute.For<ICommandHandler<ReserveStockCommand>>();
        _eventBusMock = Substitute.For<IEventBus>();
        _consumer = new ReserveStockConsumer(_handlerMock, _eventBusMock);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishStockReservedEvent_WhenCommandSucceeds()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var message = new Inventory.Contracts.IntegrationCommands.ReserveStockCommand
        {
            OrderId = orderId,
            Items = new Dictionary<Guid, int>()
        };

        _handlerMock.Handle(Arg.Any<ReserveStockCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _consumer.HandleAsync(message, CancellationToken.None);

        // Assert
        await _eventBusMock.Received(1).PublishAsync(
            Arg.Is<StockReservedEvent>(e => e.OrderId == orderId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishStockReservationFailedEvent_WhenCommandFails()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var message = new Inventory.Contracts.IntegrationCommands.ReserveStockCommand
        {
            OrderId = orderId,
            Items = new Dictionary<Guid, int>()
        };

        var expectedError = Error.BadRequest("Inventory.InsufficientStock", "Insufficient stock available.");

        _handlerMock.Handle(Arg.Any<ReserveStockCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(expectedError));

        // Act
        await _consumer.HandleAsync(message, CancellationToken.None);

        // Assert
        await _eventBusMock.Received(1).PublishAsync(
            Arg.Is<StockReservationFailedEvent>(e => 
                e.OrderId == orderId && 
                e.Reason == expectedError.Description),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishStockReservationFailedEvent_WhenExceptionIsThrown()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var message = new Inventory.Contracts.IntegrationCommands.ReserveStockCommand
        {
            OrderId = orderId,
            Items = new Dictionary<Guid, int>()
        };

        var expectedExceptionMessage = "Database connection failed.";

        _handlerMock.Handle(Arg.Any<ReserveStockCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result>(new Exception(expectedExceptionMessage)));

        // Act
        await _consumer.HandleAsync(message, CancellationToken.None);

        // Assert
        await _eventBusMock.Received(1).PublishAsync(
            Arg.Is<StockReservationFailedEvent>(e => 
                e.OrderId == orderId && 
                e.Reason == expectedExceptionMessage),
            Arg.Any<CancellationToken>());
    }
}