using BuildingBlocks.Core.Domain;
using Marten.Schema;
using System.Text.Json.Serialization;

namespace Basket.API.Domain.Models;

public class ShoppingCart : Entity<string>
{
    // The UserName serves as the ID of the cart (One cart per user)
    [Identity]
    [JsonInclude]
    public string UserName
    {
        get => Id;
        private set => Id = value;
    }

    [JsonInclude]
    public List<ShoppingCartItem> Items { get; private set; } = new();

    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);

    public ShoppingCart(string userName) : base(userName)
    {
    }

    // Default constructor for deserialization (Marten)
    public ShoppingCart() { }

    public void AddItem(Guid productId, string productName, int quantity, decimal price)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            // Assuming price could change, we might want to update it or ignore. Usually, we take the latest price.
        }
        else
        {
            Items.Add(new ShoppingCartItem(productId, productName, quantity, price));
        }
    }
}