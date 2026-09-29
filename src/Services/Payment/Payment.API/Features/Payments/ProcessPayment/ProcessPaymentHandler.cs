using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Results;
using Payment.API.Domain.Entities;
using Payment.API.Domain.Errors;
using Payment.API.Infrastructure.Data;

namespace Payment.API.Features.Payments.ProcessPayment;

internal sealed class ProcessPaymentHandler(PaymentDbContext dbContext) : ICommandHandler<ProcessPaymentCommand>
{
    public async Task<Result> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var transaction = PaymentTransaction.Create(request.OrderId, request.Amount);
        
        dbContext.PaymentTransactions.Add(transaction);

        // MOCK LOGIC: If PaymentToken contains "FAIL", we intentionally fail the payment.
        if (request.PaymentToken.Contains("FAIL", StringComparison.OrdinalIgnoreCase))
        {
            var reason = "Payment gateway rejected the token.";
            transaction.MarkAsFailed(reason);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            throw new DomainException(PaymentErrors.PaymentFailed(request.OrderId, reason));
        }

        // Simulate successful payment processing
        transaction.MarkAsCompleted();
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}