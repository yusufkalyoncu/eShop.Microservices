using BuildingBlocks.Messaging.Abstractions;

namespace Identity.Contracts.IntegrationEvents;

public sealed record UserRegisteredIntegrationEvent(
    Guid EventId,
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime RegisteredAt
) : IIntegrationEvent
{
    public static string EventName => "identity.user-registered";
}
