using System.Net;
using System.Net.Http.Json;
using Basket.API.Features.AddItemToBasket;
using Basket.API.Features.CheckoutBasket;
using Basket.API.Features.GetBasket;
using Basket.Contracts.IntegrationEvents;
using Basket.IntegrationTests.Infrastructure;
using Catalog.Grpc;
using FluentAssertions;
using Grpc.Core;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Basket.IntegrationTests.Features;

public class CheckoutBasketTests(BasketApiFactory factory) : IClassFixture<BasketApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly ITestHarness _testHarness = factory.Services.GetRequiredService<ITestHarness>();

    [Fact]
    public async Task Should_Checkout_Basket_And_Publish_Event()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var addRequest = new AddItemToBasketRequest(productId, 1);

        var mockCall = new AsyncUnaryCall<GetProductByIdResponse>(
            Task.FromResult(new GetProductByIdResponse { Id = productId.ToString(), Name = "Checkout Product", Price = 50.0 }),
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

        var checkoutRequest = new CheckoutBasketRequest(
            "John",
            "Doe",
            "john@example.com",
            "123 Main St",
            "USA",
            "NY",
            "10001",
            "tok_visa"
        );

        // Act
        var checkoutResponse = await _client.PostAsJsonAsync("/basket/checkout", checkoutRequest);

        // Assert API Response
        checkoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await checkoutResponse.Content.ReadFromJsonAsync<CheckoutBasketResponse>();
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verify Event Published
        var published = await _testHarness.Published.Any<BasketCheckoutIntegrationEvent>();
        published.Should().BeTrue();

        // Verify Basket is empty/deleted
        var getBasketResponse = await _client.GetAsync("/basket");
        var basketResult = await getBasketResponse.Content.ReadFromJsonAsync<GetBasketResponse>();
        basketResult!.Cart.Items.Should().BeEmpty();
    }
}