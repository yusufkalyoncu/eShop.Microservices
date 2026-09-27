using BuildingBlocks.Core.Domain;
using Ordering.API.Domain.Enums;
using Ordering.API.Domain.ValueObjects;

namespace Ordering.API.Domain.Models;

public class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public string UserName { get; private set; } = null!;
    public Address ShippingAddress { get; private set; } = null!;
    public Address BillingAddress { get; private set; } = null!;
    public PaymentDetails Payment { get; private set; } = null!;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal TotalPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF constructor
    protected Order() { }

    public static Order Create(Guid id, string userName, Address shippingAddress, Address billingAddress, PaymentDetails payment)
    {
        var order = new Order
        {
            Id = id,
            UserName = userName,
            ShippingAddress = shippingAddress,
            BillingAddress = billingAddress,
            Payment = payment,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        order.AddDomainEvent(new Events.OrderCreatedDomainEvent(order));
        
        return order;
    }

    public void Add(Guid productId, string productName, int quantity, decimal price)
    {
        var orderItem = new OrderItem(Id, productId, productName, quantity, price);
        _orderItems.Add(orderItem);
        
        TotalPrice += quantity * price;
    }

    public void MarkAsPaid()
    {
        Status = OrderStatus.Paid;
    }
}