using BuildingBlocks.Core.Results;

namespace Basket.API.Domain.Errors;

public static class BasketErrors
{
    public static class Cart
    {
        public static readonly Error NotFound = Error.NotFound(
            "Basket.NotFound",
            "Basket not found.");
    }

    public static class Item
    {
        public static readonly Error NotFound = Error.NotFound(
            "Item.NotFound",
            "Item not found in basket.");
    }
}
