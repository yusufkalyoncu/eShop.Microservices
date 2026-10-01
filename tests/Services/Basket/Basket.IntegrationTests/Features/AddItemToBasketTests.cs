using System.Net;
using System.Net.Http.Json;
using Basket.API.Features.AddItemToBasket;
using Basket.API.Features.GetBasket;
using Basket.IntegrationTests.Infrastructure;
using Catalog.Grpc;
using FluentAssertions;
using Grpc.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Basket.IntegrationTests.Features;

public class AddItemToBasketTests(BasketApiFactory factory) : IClassFixture<BasketApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Add_Item_To_Basket_Successfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new AddItemToBasketRequest(productId, 2);

        var mockCall = new AsyncUnaryCall<GetProductByIdResponse>(
            Task.FromResult(new GetProductByIdResponse { Id = productId.ToString(), Name = "Test Product", Price = 10.0 }),
            Task.FromResult(new Metadata()),
            () => Status.DefaultSuccess,
            () => new Metadata(),
            () => { });

        factory.CatalogGrpcClientMock
            .GetProductByIdAsync(Arg.Any<GetProductByIdRequest>(), Arg.Any<Metadata>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(mockCall);

        factory.CatalogGrpcClientMock
            .GetProductByIdAsync(Arg.Any<GetProductByIdRequest>(), Arg.Any<CallOptions>())
            .Returns(mockCall);

        _client.DefaultRequestHeaders.Remove("x-test-user");
        _client.DefaultRequestHeaders.Add("x-test-user", "add_item_user");

        // Act
        var response = await _client.PostAsJsonAsync("/basket", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // API returns 200 OK

        // Verify basket was updated
        var getBasketResponse = await _client.GetAsync("/basket");
        var basketResult = await getBasketResponse.Content.ReadFromJsonAsync<GetBasketResponse>();
        basketResult.Should().NotBeNull();
        basketResult.Cart.Items.Should().ContainSingle(i => i.ProductId == productId && i.Quantity == 2);
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new AddItemToBasketRequest(productId, 1);

        factory.CatalogGrpcClientMock
            .GetProductByIdAsync(Arg.Any<GetProductByIdRequest>(), Arg.Any<CallOptions>())
            .Throws(new RpcException(new Status(StatusCode.NotFound, "Product not found")));

        factory.CatalogGrpcClientMock
            .GetProductByIdAsync(Arg.Any<GetProductByIdRequest>(), Arg.Any<Metadata>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Throws(new RpcException(new Status(StatusCode.NotFound, "Product not found")));

        _client.DefaultRequestHeaders.Remove("x-test-user");
        _client.DefaultRequestHeaders.Add("x-test-user", "add_item_fail_user");

        // Act
        var response = await _client.PostAsJsonAsync("/basket", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}