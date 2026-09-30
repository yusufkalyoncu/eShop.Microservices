using BuildingBlocks.Core.Domain.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.API.Domain.Enums;
using Payment.API.Features.Payments.ProcessPayment;
using Payment.API.Infrastructure.Data;

namespace Payment.UnitTests.Features.Payments;

public class ProcessPaymentHandlerTests
{
    private readonly PaymentDbContext _dbContext;
    private readonly ProcessPaymentHandler _handler;

    public ProcessPaymentHandlerTests()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _dbContext = new PaymentDbContext(options);
        _handler = new ProcessPaymentHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_ShouldCompletePaymentAndSave_WhenTokenIsValid()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(orderId, 150.0m, "tok_valid");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var transaction = await _dbContext.PaymentTransactions.FirstOrDefaultAsync(t => t.OrderId == orderId);
        transaction.Should().NotBeNull();
        transaction.Status.Should().Be(PaymentStatus.Completed);
        transaction.Amount.Should().Be(150.0m);
    }

    [Fact]
    public async Task Handle_ShouldFailPaymentSaveAndThrowDomainException_WhenTokenContainsFail()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(orderId, 200.0m, "tok_FAIL_123");

        // Act
        Func<Task> action = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DomainException>()
            .WithMessage("*Payment failed*");

        var transaction = await _dbContext.PaymentTransactions.FirstOrDefaultAsync(t => t.OrderId == orderId);
        transaction.Should().NotBeNull();
        transaction.Status.Should().Be(PaymentStatus.Failed);
        transaction.FailureReason.Should().Be("Payment gateway rejected the token.");
    }
}