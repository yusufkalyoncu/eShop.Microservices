using FluentAssertions;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payment.API.Domain.Enums;
using Payment.API.Infrastructure.Data;
using Payment.Contracts.IntegrationCommands;
using Payment.Contracts.IntegrationEvents;
using Payment.IntegrationTests.Infrastructure;

namespace Payment.IntegrationTests.Features.Consumers;

public class ProcessPaymentConsumerTests(PaymentApiFactory factory) : IClassFixture<PaymentApiFactory>
{
    private readonly ITestHarness _testHarness = factory.Services.GetRequiredService<ITestHarness>();

    [Fact]
    public async Task Should_Process_Payment_Successfully_And_Publish_SucceededEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ProcessPaymentIntegrationCommand
        {
            OrderId = orderId,
            Amount = 100.50m,
            PaymentToken = "tok_success"
        };

        // Act
        await _testHarness.Bus.Publish(command);

        // Assert
        var consumed = await _testHarness.Consumed.Any<ProcessPaymentIntegrationCommand>();
        consumed.Should().BeTrue();

        await Task.Delay(500); // Allow time for Consumer and Outbox/EF to process

        var published = await _testHarness.Published.Any<PaymentSucceededEvent>(e => e.Context != null && e.Context.Message.OrderId == orderId);
        published.Should().BeTrue();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        var transaction = await dbContext.PaymentTransactions.FirstOrDefaultAsync(t => t.OrderId == orderId);
            
        transaction.Should().NotBeNull();
        transaction.Amount.Should().Be(100.50m);
        transaction.Status.Should().Be(PaymentStatus.Completed);
    }

    [Fact]
    public async Task Should_Fail_Payment_When_Token_Is_Invalid_And_Publish_FailedEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ProcessPaymentIntegrationCommand
        {
            OrderId = orderId,
            Amount = 50.00m,
            PaymentToken = "tok_FAIL_123"
        };

        // Act
        await _testHarness.Bus.Publish(command);

        // Assert
        var consumed = await _testHarness.Consumed.Any<ProcessPaymentIntegrationCommand>();
        consumed.Should().BeTrue();

        await Task.Delay(500); // Allow time for Consumer and Outbox/EF to process

        var published = await _testHarness.Published.Any<PaymentFailedEvent>(e => e.Context != null && e.Context.Message.OrderId == orderId);
        published.Should().BeTrue();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        var transaction = await dbContext.PaymentTransactions.FirstOrDefaultAsync(t => t.OrderId == orderId);
            
        transaction.Should().NotBeNull();
        transaction.Amount.Should().Be(50.00m);
        transaction.Status.Should().Be(PaymentStatus.Failed);
        transaction.FailureReason.Should().NotBeNullOrEmpty();
    }
}