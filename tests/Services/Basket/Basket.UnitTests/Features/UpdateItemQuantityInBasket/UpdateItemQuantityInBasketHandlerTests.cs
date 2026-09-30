using Basket.API.Domain.Errors;
using Basket.API.Domain.Models;
using Basket.API.Features.UpdateItemQuantityInBasket;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace Basket.UnitTests.Features.UpdateItemQuantityInBasket;

public class UpdateItemQuantityInBasketHandlerTests
{
    private readonly IDocumentSession _sessionMock;
    private readonly UpdateItemQuantityInBasketHandler _handler;

    public UpdateItemQuantityInBasketHandlerTests()
    {
        _sessionMock = Substitute.For<IDocumentSession>();
        _handler = new UpdateItemQuantityInBasketHandler(_sessionMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCartNotFound()
    {
        // Arrange
        var command = new UpdateItemQuantityInBasketCommand("testuser", Guid.NewGuid(), 2);

        _sessionMock.LoadAsync<ShoppingCart>(command.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BasketErrors.Cart.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenItemNotFoundInCart()
    {
        // Arrange
        var command = new UpdateItemQuantityInBasketCommand("testuser", Guid.NewGuid(), 2);
        var cart = new ShoppingCart(command.UserName);
        cart.AddItem(Guid.NewGuid(), "Other Product", 1, 10.0m); // Different product

        _sessionMock.LoadAsync<ShoppingCart>(command.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(cart));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BasketErrors.Item.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldRemoveItemAndSave_WhenQuantityIsZeroOrLess()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new UpdateItemQuantityInBasketCommand("testuser", productId, 0);
        var cart = new ShoppingCart(command.UserName);
        cart.AddItem(productId, "Product A", 2, 10.0m);

        _sessionMock.LoadAsync<ShoppingCart>(command.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(cart));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.IsSuccess.Should().BeTrue();

        cart.Items.Should().BeEmpty(); // Since it was the only item and it got removed

        _sessionMock.Received(1).Update(cart);
        await _sessionMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateQuantityAndSave_WhenQuantityIsGreaterThanZero()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var newQuantity = 5;
        var command = new UpdateItemQuantityInBasketCommand("testuser", productId, newQuantity);
        var cart = new ShoppingCart(command.UserName);
        cart.AddItem(productId, "Product A", 2, 10.0m);

        _sessionMock.LoadAsync<ShoppingCart>(command.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(cart));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.IsSuccess.Should().BeTrue();

        cart.Items.First().Quantity.Should().Be(newQuantity);

        _sessionMock.Received(1).Update(cart);
        await _sessionMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}