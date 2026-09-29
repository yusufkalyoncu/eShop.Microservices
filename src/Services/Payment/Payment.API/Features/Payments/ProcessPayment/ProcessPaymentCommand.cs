using BuildingBlocks.Core.CQRS;

namespace Payment.API.Features.Payments.ProcessPayment;

public record ProcessPaymentCommand(Guid OrderId, decimal Amount, string PaymentToken) : ICommand;