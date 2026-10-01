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

public class RemoveItemFromBasketTests(BasketApiFactory factory) : IClassFixture<BasketApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Should_Remove_Item_From_Basket_Successfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var addRequest = new AddItemToBasketRequest(productId, 1);

        var mockCall = new AsyncUnaryCall<GetProductByIdResponse>(
            Task.FromResult(new GetProductByIdResponse { Id = productId.ToString(), Name = "Remove Product", Price = 15.0 }),
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

        var addResponse = await _client.PostAsJsonAsync("/basket", addRequest);
        addResponse.EnsureSuccessStatusCode();

        // Act
        var removeResponse = await _client.DeleteAsync($"/basket/items/{productId}");

        // Assert
        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify basket is empty
        var getBasketResponse = await _client.GetAsync("/basket");
        var basketResult = await getBasketResponse.Content.ReadFromJsonAsync<GetBasketResponse>();
        basketResult.Should().NotBeNull();
        basketResult!.Cart.Items.Should().BeEmpty();
    }
}