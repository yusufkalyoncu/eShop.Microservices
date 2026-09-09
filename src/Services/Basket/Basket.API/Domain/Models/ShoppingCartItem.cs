using BuildingBlocks.Core.Domain;
using System.Text.Json.Serialization;

namespace Basket.API.Domain.Models;

public class ShoppingCartItem : Entity<Guid>
{
    [JsonInclude]
    public int Quantity { get; private set; }
    
    [JsonInclude]
    public decimal Price { get; private set; }
    
    [JsonInclude]
    public Guid ProductId { get; private set; }
    
    [JsonInclude]
    public string ProductName { get; private set; } = null!;

    public ShoppingCartItem(Guid productId, string productName, int quantity, decimal price) : base(Guid.NewGuid())
    {
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }

    // Default constructor for deserialization
    public ShoppingCartItem() { }

    public void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
    }
}