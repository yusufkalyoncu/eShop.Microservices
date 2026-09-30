using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Security;

namespace Basket.API.Features.CheckoutBasket;

public record CheckoutBasketRequest(
    string FirstName,
    string LastName,
    string EmailAddress,
    string AddressLine,
    string Country,
    string State,
    string ZipCode,
    string PaymentToken);

public record CheckoutBasketResponse(bool IsSuccess);

public class CheckoutBasketEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/basket/checkout", async (CheckoutBasketRequest request, ICurrentUser currentUser, ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult> handler, CancellationToken ct) =>
        {
            var userName = currentUser.GetRequiredName();
            
            var command = new CheckoutBasketCommand(
                userName, request.FirstName, request.LastName, request.EmailAddress, request.AddressLine, 
                request.Country, request.State, request.ZipCode, request.PaymentToken);
                
            var result = await handler.Handle(command, ct);
            
            return result.Match(res => new CheckoutBasketResponse(res.IsSuccess));
        })
        .WithName("CheckoutBasket")
        .Produces<CheckoutBasketResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Checkout Basket")
        .WithDescription("Checkout the basket and create an order")
        .RequireAuthorization();
    }
}