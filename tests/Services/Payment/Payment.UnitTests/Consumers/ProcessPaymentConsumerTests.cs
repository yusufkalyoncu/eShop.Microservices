using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using BuildingBlocks.Messaging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Payment.API.Consumers;
using Payment.API.Features.Payments.ProcessPayment;
using Payment.Contracts.IntegrationCommands;
using Payment.Contracts.IntegrationEvents;

namespace Payment.UnitTests.Consumers;

public class ProcessPaymentConsumerTests
{
    private readonly ICommandHandler<ProcessPaymentCommand> _handlerMock;
    private readonly IEventBus _eventBusMock;
    private readonly ProcessPaymentConsumer _consumer;

    public ProcessPaymentConsumerTests()
    {
        _handlerMock = Substitute.For<ICommandHandler<ProcessPaymentCommand>>();
        _eventBusMock = Substitute.For<IEventBus>();
        _consumer = new ProcessPaymentConsumer(_handlerMock, _eventBusMock);
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishPaymentSucceededEvent_WhenHandlerSucceeds()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var @event = new ProcessPaymentIntegrationCommand
        {
            OrderId = orderId,
            Amount = 150.0m,
            PaymentToken = "tok_valid"
        };

        _handlerMock.Handle(Arg.Any<ProcessPaymentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _consumer.HandleAsync(@event, CancellationToken.None);

        // Assert
        await _eventBusMock.Received(1).PublishAsync(
            Arg.Is<PaymentSucceededEvent>(e => e.OrderId == orderId),
            Arg.Any<CancellationToken>());
        
        await _eventBusMock.DidNotReceive().PublishAsync(
            Arg.Any<PaymentFailedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishPaymentFailedEvent_WhenHandlerReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var @event = new ProcessPaymentIntegrationCommand
        {
            OrderId = orderId,
            Amount = 150.0m,
            PaymentToken = "tok_invalid"
        };

        _handlerMock.Handle(Arg.Any<ProcessPaymentCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(Error.BadRequest("Code", "Some failure reason")));

        // Act
        await _consumer.HandleAsync(@event, CancellationToken.None);

        // Assert
        await _eventBusMock.Received(1).PublishAsync(
            Arg.Is<PaymentFailedEvent>(e => e.OrderId == orderId && e.Reason == "Some failure reason"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ShouldPublishPaymentFailedEvent_WhenHandlerThrowsException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var @event = new ProcessPaymentIntegrationCommand
        {
            OrderId = orderId,
            Amount = 150.0m,
            PaymentToken = "tok_FAIL"
        };

        _handlerMock.Handle(Arg.Any<ProcessPaymentCommand>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Database exploded"));

        // Act
        await _consumer.HandleAsync(@event, CancellationToken.None);

        // Assert
        await _eventBusMock.Received(1).PublishAsync(
            Arg.Is<PaymentFailedEvent>(e => e.OrderId == orderId && e.Reason == "Database exploded"),
            Arg.Any<CancellationToken>());
    }
}