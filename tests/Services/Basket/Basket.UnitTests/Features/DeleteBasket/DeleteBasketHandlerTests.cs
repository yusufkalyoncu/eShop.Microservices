using Basket.API.Domain.Models;
using Basket.API.Features.DeleteBasket;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace Basket.UnitTests.Features.DeleteBasket;

public class DeleteBasketHandlerTests
{
    private readonly IDocumentSession _sessionMock;
    private readonly DeleteBasketCommandHandler _handler;

    public DeleteBasketHandlerTests()
    {
        _sessionMock = Substitute.For<IDocumentSession>();
        _handler = new DeleteBasketCommandHandler(_sessionMock);
    }

    [Fact]
    public async Task Handle_ShouldDeleteBasketAndSaveChanges()
    {
        // Arrange
        var command = new DeleteBasketCommand("testuser");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.IsSuccess.Should().BeTrue();

        _sessionMock.Received(1).Delete<ShoppingCart>(command.UserName);
        await _sessionMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}