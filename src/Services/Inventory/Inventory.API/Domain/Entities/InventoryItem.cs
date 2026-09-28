using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Domain.Exceptions;
using Inventory.API.Domain.Errors;
using Inventory.API.Domain.ValueObjects;

namespace Inventory.API.Domain.Entities;

public sealed class InventoryItem : AggregateRoot<Guid>
{
    public Guid ProductId { get; private set; }
    public Quantity AvailableQuantity { get; private set; } = null!;
    
    // EF Core constructor
    private InventoryItem() { }

    private InventoryItem(Guid id, Guid productId, Quantity availableQuantity)
    {
        Id = id;
        ProductId = productId;
        AvailableQuantity = availableQuantity;
    }

    public static InventoryItem Create(Guid productId, int initialQuantity = 0)
    {
        return new InventoryItem(Guid.NewGuid(), productId, Quantity.Create(initialQuantity));
    }

    public void AddStock(int quantity)
    {
        AvailableQuantity = AvailableQuantity.Add(quantity);
    }

    public void ReserveStock(int quantity)
    {
        if (AvailableQuantity.Value < quantity)
        {
            throw new DomainException(InventoryErrors.InsufficientStock(ProductId));
        }

        AvailableQuantity = AvailableQuantity.Subtract(quantity);
    }

    public void ReleaseStock(int quantity)
    {
        AvailableQuantity = AvailableQuantity.Add(quantity);
    }
}