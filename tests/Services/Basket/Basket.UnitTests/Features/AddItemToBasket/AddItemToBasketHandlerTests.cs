using Basket.API.Domain.Models;
using Basket.API.Features.AddItemToBasket;
using BuildingBlocks.Core.Domain.Exceptions;
using Catalog.Grpc;
using FluentAssertions;
using Grpc.Core;
using Marten;
using NSubstitute;

namespace Basket.UnitTests.Features.AddItemToBasket;

public class AddItemToBasketHandlerTests
{
    private readonly IDocumentSession _sessionMock;
    private readonly CatalogGrpcService.CatalogGrpcServiceClient _catalogClientMock;
    private readonly AddItemToBasketCommandHandler _handler;

    public AddItemToBasketHandlerTests()
    {
        _sessionMock = Substitute.For<IDocumentSession>();
        _catalogClientMock = Substitute.For<CatalogGrpcService.CatalogGrpcServiceClient>();
        _handler = new AddItemToBasketCommandHandler(_sessionMock, _catalogClientMock);
    }

    private AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(TResponse response)
    {
        return new AsyncUnaryCall<TResponse>(
            Task.FromResult(response),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => [],
            () => { });
    }

    private AsyncUnaryCall<TResponse> CreateAsyncUnaryCallException<TResponse>(RpcException exception)
    {
        return new AsyncUnaryCall<TResponse>(
            Task.FromException<TResponse>(exception),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => [],
            () => { });
    }

    [Fact]
    public async Task Handle_ShouldAddNewItem_WhenCartDoesNotExist()
    {
        // Arrange
        var command = new AddItemToBasketCommand("testuser", Guid.NewGuid(), 2);
        var productResponse = new GetProductByIdResponse
        {
            Id = command.ProductId.ToString(),
            Name = "Test Product",
            Price = 50.0
        };

        _catalogClientMock.GetProductByIdAsync(Arg.Any<GetProductByIdRequest>(), null, null, Arg.Any<CancellationToken>())
            .Returns(CreateAsyncUnaryCall(productResponse));

        _sessionMock.LoadAsync<ShoppingCart>(command.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.UserName.Should().Be(command.UserName);

        _sessionMock.Received(1).Store(Arg.Is<ShoppingCart>(c => 
            c.UserName == command.UserName && 
            c.Items.Count == 1 &&
            c.Items.First().ProductId == command.ProductId));
            
        await _sessionMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateCart_WhenCartAlreadyExists()
    {
        // Arrange
        var command = new AddItemToBasketCommand("testuser", Guid.NewGuid(), 3);
        var productResponse = new GetProductByIdResponse
        {
            Id = command.ProductId.ToString(),
            Name = "Existing Product",
            Price = 100.0
        };

        var existingCart = new ShoppingCart(command.UserName);
        existingCart.AddItem(command.ProductId, "Existing Product", 1, 100.0m);

        _catalogClientMock.GetProductByIdAsync(Arg.Any<GetProductByIdRequest>(), null, null, Arg.Any<CancellationToken>())
            .Returns(CreateAsyncUnaryCall(productResponse));

        _sessionMock.LoadAsync<ShoppingCart>(command.UserName, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ShoppingCart?>(existingCart));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _sessionMock.Received(1).Store(Arg.Is<ShoppingCart>(c => 
            c.UserName == command.UserName && 
            c.Items.Count == 1 &&
            c.Items.First().Quantity == 4)); // 1 existing + 3 new

        await _sessionMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowDomainException_WhenProductNotFound()
    {
        // Arrange
        var command = new AddItemToBasketCommand("testuser", Guid.NewGuid(), 1);

        var rpcException = new RpcException(new Status(StatusCode.NotFound, "Product not found"));

        _catalogClientMock.GetProductByIdAsync(Arg.Any<GetProductByIdRequest>(), null, null, Arg.Any<CancellationToken>())
            .Returns(CreateAsyncUnaryCallException<GetProductByIdResponse>(rpcException));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage($"*not found*");
            
        _sessionMock.DidNotReceive().Store(Arg.Any<ShoppingCart>());
    }
}
