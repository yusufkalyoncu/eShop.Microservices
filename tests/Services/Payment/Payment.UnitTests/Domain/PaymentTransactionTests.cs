using FluentAssertions;
using Payment.API.Domain.Entities;
using Payment.API.Domain.Enums;

namespace Payment.UnitTests.Domain;

public class PaymentTransactionTests
{
    [Fact]
    public void Create_ShouldInitializeCorrectly_WithPendingStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 150.75m;

        // Act
        var transaction = PaymentTransaction.Create(orderId, amount);

        // Assert
        transaction.Id.Should().NotBeEmpty();
        transaction.OrderId.Should().Be(orderId);
        transaction.Amount.Should().Be(amount);
        transaction.Status.Should().Be(PaymentStatus.Pending);
        transaction.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        transaction.FailureReason.Should().BeNull();
    }

    [Fact]
    public async Task MarkAsCompleted_ShouldSetStatusToCompletedAndSetProcessedAt()
    {
        // Arrange
        var transaction = PaymentTransaction.Create(Guid.NewGuid(), 100m);
        var originalProcessedAt = transaction.ProcessedAt;
        
        // Wait briefly to ensure ProcessedAt changes
        await Task.Delay(10);

        // Act
        transaction.MarkAsCompleted();

        // Assert
        transaction.Status.Should().Be(PaymentStatus.Completed);
        transaction.ProcessedAt.Should().BeAfter(originalProcessedAt);
        transaction.FailureReason.Should().BeNull();
    }

    [Fact]
    public async Task MarkAsFailed_ShouldSetStatusToFailedAndSetFailureReason()
    {
        // Arrange
        var transaction = PaymentTransaction.Create(Guid.NewGuid(), 100m);
        var originalProcessedAt = transaction.ProcessedAt;
        var failureReason = "Insufficient funds";
        
        await Task.Delay(10);

        // Act
        transaction.MarkAsFailed(failureReason);

        // Assert
        transaction.Status.Should().Be(PaymentStatus.Failed);
        transaction.FailureReason.Should().Be(failureReason);
        transaction.ProcessedAt.Should().BeAfter(originalProcessedAt);
    }
}