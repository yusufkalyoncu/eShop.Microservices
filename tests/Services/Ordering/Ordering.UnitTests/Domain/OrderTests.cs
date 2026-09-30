using FluentAssertions;
using Ordering.API.Domain.Enums;
using Ordering.API.Domain.Events;
using Ordering.API.Domain.Models;
using Ordering.API.Domain.ValueObjects;

namespace Ordering.UnitTests.Domain;

public class OrderTests
{
    private Address CreateSampleAddress() =>
        new("John", "Doe", "john@test.com", "123 Main St", "USA", "NY", "10001");

    private PaymentDetails CreateSamplePayment() =>
        new("token_123");

    [Fact]
    public void Create_ShouldInitializeOrderAndAddDomainEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var address = CreateSampleAddress();
        var payment = CreateSamplePayment();

        // Act
        var order = Order.Create(orderId, "johndoe", address, address, payment);

        // Assert
        order.Id.Should().Be(orderId);
        order.UserName.Should().Be("johndoe");
        order.Status.Should().Be(OrderStatus.Pending);
        order.ShippingAddress.Should().Be(address);
        order.BillingAddress.Should().Be(address);
        order.Payment.Should().Be(payment);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        // Domain Event Assert
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCreatedDomainEvent>();
    }

    [Fact]
    public void Add_ShouldAddOrderItemAndIncreaseTotalPrice()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), "user", CreateSampleAddress(), CreateSampleAddress(), CreateSamplePayment());

        // Act
        order.Add(Guid.NewGuid(), "Product 1", 2, 50.0m);
        order.Add(Guid.NewGuid(), "Product 2", 1, 100.0m);

        // Assert
        order.OrderItems.Should().HaveCount(2);
        order.TotalPrice.Should().Be(200.0m); // (2 * 50) + (1 * 100)
    }

    [Fact]
    public void MarkAsPaid_ShouldChangeStatusToPaid()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), "user", CreateSampleAddress(), CreateSampleAddress(), CreateSamplePayment());

        // Act
        order.MarkAsPaid();

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void MarkAsCancelled_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), "user", CreateSampleAddress(), CreateSampleAddress(), CreateSamplePayment());

        // Act
        order.MarkAsCancelled();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }
}