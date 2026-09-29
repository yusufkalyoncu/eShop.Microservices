using BuildingBlocks.Core.Results;

namespace Payment.API.Domain.Errors;

public static class PaymentErrors
{
    public static Error PaymentFailed(Guid orderId, string reason) => 
        Error.BadRequest("Payment.Failed", $"Payment failed for order '{orderId}'. Reason: {reason}");
}