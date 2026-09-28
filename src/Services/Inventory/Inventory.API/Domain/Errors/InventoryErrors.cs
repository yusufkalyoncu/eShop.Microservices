using BuildingBlocks.Core.Results;

namespace Inventory.API.Domain.Errors;

public static class InventoryErrors
{
    public static Error InsufficientStock(Guid productId) => 
        Error.Conflict("Inventory.InsufficientStock", $"Insufficient stock for product '{productId}'.");
        
    public static Error ProductNotFound(Guid productId) => 
        Error.NotFound("Inventory.ProductNotFound", $"Inventory for product '{productId}' was not found.");
        
    public static Error InvalidQuantity(int quantity) => 
        Error.BadRequest("Inventory.InvalidQuantity", $"Quantity '{quantity}' is invalid. Must be non-negative.");
}