using BuildingBlocks.Core.Domain;

namespace Ordering.API.Domain.Models;

public class OrderItem : Entity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal Price { get; private set; }

    // EF Core constructor
    protected OrderItem() { }

    internal OrderItem(Guid orderId, Guid productId, string productName, int quantity, decimal price)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
    }
}