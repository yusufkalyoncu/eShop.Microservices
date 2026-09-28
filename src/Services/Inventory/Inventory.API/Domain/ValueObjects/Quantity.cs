using BuildingBlocks.Core.Domain.Exceptions;
using Inventory.API.Domain.Errors;

namespace Inventory.API.Domain.ValueObjects;

public sealed record Quantity
{
    public int Value { get; }

    private Quantity(int value) => Value = value;

    public static Quantity Create(int value)
    {
        if (value < 0)
        {
            throw new DomainException(InventoryErrors.InvalidQuantity(value));
        }

        return new Quantity(value);
    }
    
    public Quantity Add(int amount)
    {
        return Create(Value + amount);
    }
    
    public Quantity Subtract(int amount)
    {
        return Create(Value - amount);
    }

    public static implicit operator int(Quantity quantity) => quantity.Value;
}