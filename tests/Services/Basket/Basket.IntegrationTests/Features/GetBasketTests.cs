using System.Net;
using System.Net.Http.Json;
using Basket.API.Features.AddItemToBasket;
using Basket.API.Features.GetBasket;
using Basket.IntegrationTests.Infrastructure;
using Catalog.Grpc;
using FluentAssertions;
using Grpc.Core;
using NSubstitute;

namespace Basket.IntegrationTests.Features;

public class GetBasketTests(BasketApiFactory factory) : IClassFixture<BasketApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Return_Empty_Basket_When_Not_Exists()
    {
        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/basket");
        request.Headers.Add("x-test-user", "emptyuser");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<GetBasketResponse>();
        result.Should().NotBeNull();
        result.Cart.UserName.Should().Be("emptyuser");
        result.Cart.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_Return_Existing_Basket_With_Items()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var addRequest = new AddItemToBasketRequest(productId, 5);

        var mockCall = new AsyncUnaryCall<GetProductByIdResponse>(
            Task.FromResult(new GetProductByIdResponse { Id = productId.ToString(), Name = "Get Product", Price = 20.0 }),
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

        var addRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/basket");
        addRequestMessage.Headers.Add("x-test-user", "getbasketuser");
        addRequestMessage.Content = JsonContent.Create(addRequest);
        var addResponse = await _client.SendAsync(addRequestMessage);
        addResponse.EnsureSuccessStatusCode();

        // Act
        var getRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/basket");
        getRequestMessage.Headers.Add("x-test-user", "getbasketuser");
        var response = await _client.SendAsync(getRequestMessage);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<GetBasketResponse>();
        result.Should().NotBeNull();
        result.Cart.UserName.Should().Be("getbasketuser");
        result.Cart.Items.Should().ContainSingle(i => i.ProductId == productId && i.Quantity == 5);
        result.Cart.TotalPrice.Should().Be(100.0m);
    }
}