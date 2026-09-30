using Basket.API.Domain.Errors;
using Basket.API.Domain.Models;
using Basket.API.Features.CheckoutBasket;
using Basket.Contracts.IntegrationEvents;
using FluentAssertions;
using Marten;
using MassTransit;
using NSubstitute;

namespace Basket.UnitTests.Features.CheckoutBasket;

public class CheckoutBasketHandlerTests
{
    private readonly IDocumentSession _sessionMock;
    private readonly IPublishEndpoint _publishEndpointMock;
    private readonly CheckoutBasketCommandHandler _handler;

    public CheckoutBasketHandlerTests()
    {
        _sessionMock = Substitute.For<IDocumentSession>();
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
        _handler = new CheckoutBasketCommandHandler(_sessionMock, _publishEndpointMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenBasketIsNull()
    {
        // Arrange
        var command = new CheckoutBasketCommand("testuser", "John", "Doe", "john@test.com", "123 Main St", "USA", "NY", "10001", "token123");

        _sessionMock.LoadAsync<ShoppingCart>(command.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BasketErrors.Cart.Empty);

        await _publishEndpointMock.DidNotReceiveWithAnyArgs().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
        _sessionMock.DidNotReceiveWithAnyArgs().Delete(Arg.Any<ShoppingCart>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenBasketIsEmpty()
    {
        // Arrange
        var command = new CheckoutBasketCommand("testuser", "John", "Doe", "john@test.com", "123 Main St", "USA", "NY", "10001", "token123");

        var emptyBasket = new ShoppingCart(command.UserName);

        _sessionMock.LoadAsync<ShoppingCart>(command.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(emptyBasket));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BasketErrors.Cart.Empty);

        await _publishEndpointMock.DidNotReceiveWithAnyArgs().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
        _sessionMock.DidNotReceiveWithAnyArgs().Delete(Arg.Any<ShoppingCart>());
    }

    [Fact]
    public async Task Handle_ShouldPublishEventAndDeleteBasket_WhenBasketHasItems()
    {
        // Arrange
        var command = new CheckoutBasketCommand("testuser", "John", "Doe", "john@test.com", "123 Main St", "USA", "NY", "10001", "token123");

        var basket = new ShoppingCart(command.UserName);
        basket.AddItem(Guid.NewGuid(), "Test Product", 2, 50.0m);

        _sessionMock.LoadAsync<ShoppingCart>(command.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(basket));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.IsSuccess.Should().BeTrue();

        await _publishEndpointMock.Received(1).Publish(
            Arg.Is<BasketCheckoutIntegrationEvent>(e => 
                e.UserName == command.UserName && 
                e.TotalPrice == 100.0m &&
                e.Items.Count == 1), 
            Arg.Any<CancellationToken>());

        _sessionMock.Received(1).Delete(basket);
        await _sessionMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}