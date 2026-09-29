using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Web.Endpoints;
using BuildingBlocks.Web.Extensions;

namespace Payment.API.Features.Payments.ProcessPayment;

public record ProcessPaymentRequest(Guid OrderId, decimal Amount, string PaymentToken);

public sealed class ProcessPaymentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/process", async (ProcessPaymentRequest request, ICommandHandler<ProcessPaymentCommand> handler, CancellationToken ct) =>
        {
            var command = new ProcessPaymentCommand(
                request.OrderId, 
                request.Amount, 
                request.PaymentToken);

            var result = await handler.Handle(command, ct);
            return result.Match();
        })
        .WithTags("Payments")
        .WithSummary("Process a mock payment")
        .WithDescription("Processes a payment using a token. Send 'FAIL' in token to simulate a failed payment.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}