using Basket.API.Domain.Models;
using Basket.API.Features.GetBasket;
using FluentAssertions;
using Marten;
using NSubstitute;

namespace Basket.UnitTests.Features.GetBasket;

public class GetBasketHandlerTests
{
    private readonly IDocumentSession _sessionMock;
    private readonly GetBasketQueryHandler _handler;

    public GetBasketHandlerTests()
    {
        _sessionMock = Substitute.For<IDocumentSession>();
        _handler = new GetBasketQueryHandler(_sessionMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnExistingBasket_WhenBasketExists()
    {
        // Arrange
        var query = new GetBasketQuery("testuser");
        var existingBasket = new ShoppingCart(query.UserName);
        existingBasket.AddItem(Guid.NewGuid(), "Product A", 1, 10.0m);

        _sessionMock.LoadAsync<ShoppingCart>(query.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(existingBasket));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Cart.UserName.Should().Be(query.UserName);
        result.Data.Cart.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnNewBasket_WhenBasketDoesNotExist()
    {
        // Arrange
        var query = new GetBasketQuery("testuser");

        _sessionMock.LoadAsync<ShoppingCart>(query.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Cart.Should().NotBeNull();
        result.Data.Cart.UserName.Should().Be(query.UserName);
        result.Data.Cart.Items.Should().BeEmpty();
    }
}