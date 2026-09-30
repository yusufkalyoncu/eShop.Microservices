using Basket.API.Domain.Errors;
using Basket.API.Domain.Models;
using Basket.API.Features.RemoveItemFromBasket;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace Basket.UnitTests.Features.RemoveItemFromBasket;

public class RemoveItemFromBasketHandlerTests
{
    private readonly IDocumentSession _sessionMock;
    private readonly RemoveItemFromBasketHandler _handler;

    public RemoveItemFromBasketHandlerTests()
    {
        _sessionMock = Substitute.For<IDocumentSession>();
        _handler = new RemoveItemFromBasketHandler(_sessionMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCartNotFound()
    {
        // Arrange
        var command = new RemoveItemFromBasketCommand("testuser", Guid.NewGuid());

        _sessionMock.LoadAsync<ShoppingCart>(command.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BasketErrors.Cart.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldRemoveItemAndSave_WhenItemExistsInCart()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new RemoveItemFromBasketCommand("testuser", productId);
        var cart = new ShoppingCart(command.UserName);
        cart.AddItem(productId, "Product A", 1, 10.0m);
        cart.AddItem(Guid.NewGuid(), "Product B", 2, 20.0m); // Another item to ensure cart is not empty

        _sessionMock.LoadAsync<ShoppingCart>(command.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(cart));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.IsSuccess.Should().BeTrue();

        cart.Items.Should().HaveCount(1);
        cart.Items.Should().NotContain(x => x.ProductId == productId);

        _sessionMock.Received(1).Update(cart);
        await _sessionMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenItemDoesNotExistInCart()
    {
        // Arrange
        var command = new RemoveItemFromBasketCommand("testuser", Guid.NewGuid());
        var cart = new ShoppingCart(command.UserName);
        cart.AddItem(Guid.NewGuid(), "Product A", 1, 10.0m);

        _sessionMock.LoadAsync<ShoppingCart>(command.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(cart));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.IsSuccess.Should().BeFalse();

        _sessionMock.DidNotReceiveWithAnyArgs().Update(Arg.Any<ShoppingCart>());
    }
}