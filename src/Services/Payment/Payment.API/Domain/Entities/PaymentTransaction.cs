using BuildingBlocks.Core.Domain;
using Payment.API.Domain.Enums;

namespace Payment.API.Domain.Entities;

public sealed class PaymentTransaction : AggregateRoot<Guid>
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }

    // EF Core constructor
    private PaymentTransaction() { }

    private PaymentTransaction(Guid id, Guid orderId, decimal amount)
    {
        Id = id;
        OrderId = orderId;
        Amount = amount;
        Status = PaymentStatus.Pending;
        ProcessedAt = DateTime.UtcNow;
    }

    public static PaymentTransaction Create(Guid orderId, decimal amount)
    {
        return new PaymentTransaction(Guid.NewGuid(), orderId, amount);
    }

    public void MarkAsCompleted()
    {
        Status = PaymentStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        ProcessedAt = DateTime.UtcNow;
    }
}