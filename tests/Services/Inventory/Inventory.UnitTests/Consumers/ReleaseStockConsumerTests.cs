using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Results;
using Inventory.API.Consumers;
using Inventory.API.Features.Stock.ReleaseStock;
using NSubstitute;

namespace Inventory.UnitTests.Consumers;

public class ReleaseStockConsumerTests
{
    private readonly ICommandHandler<ReleaseStockCommand> _handlerMock;
    private readonly ReleaseStockConsumer _consumer;

    public ReleaseStockConsumerTests()
    {
        _handlerMock = Substitute.For<ICommandHandler<ReleaseStockCommand>>();
        _consumer = new ReleaseStockConsumer(_handlerMock);
    }

    [Fact]
    public async Task HandleAsync_ShouldInvokeHandlerWithMappedCommand()
    {
        // Arrange
        var message = new Inventory.Contracts.IntegrationCommands.ReleaseStockCommand
        {
            OrderId = Guid.NewGuid(),
            Items = new Dictionary<Guid, int> { { Guid.NewGuid(), 5 } }
        };

        _handlerMock.Handle(Arg.Any<ReleaseStockCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _consumer.HandleAsync(message, CancellationToken.None);

        // Assert
        await _handlerMock.Received(1).Handle(
            Arg.Is<ReleaseStockCommand>(c => c.Items == message.Items),
            Arg.Any<CancellationToken>());
    }
}