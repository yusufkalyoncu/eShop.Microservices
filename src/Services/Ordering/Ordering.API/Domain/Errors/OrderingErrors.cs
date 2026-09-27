using BuildingBlocks.Core.Results;

namespace Ordering.API.Domain.Errors;

public static class OrderingErrors
{
    public static class Order
    {
        public static readonly Error NotFound = Error.NotFound(
            "Order.NotFound",
            "Order not found.");
    }
}