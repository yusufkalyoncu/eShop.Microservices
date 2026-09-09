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
}