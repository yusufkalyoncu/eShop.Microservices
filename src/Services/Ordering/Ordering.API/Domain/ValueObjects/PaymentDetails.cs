namespace Ordering.API.Domain.ValueObjects;

public record PaymentDetails(
    string CardName,
    string CardNumber,
    string Expiration,
    string CVV,
    int PaymentMethod);