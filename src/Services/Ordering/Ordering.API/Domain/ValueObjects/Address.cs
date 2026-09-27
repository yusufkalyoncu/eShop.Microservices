namespace Ordering.API.Domain.ValueObjects;

public record Address(
    string FirstName,
    string LastName,
    string EmailAddress,
    string AddressLine,
    string Country,
    string State,
    string ZipCode);