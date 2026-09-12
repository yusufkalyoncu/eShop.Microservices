using BuildingBlocks.Messaging.Abstractions;
using Identity.Contracts.IntegrationEvents;
using Notification.API.Application.Strategies;
using Notification.API.Domain.Enums;

namespace Notification.API.Application.IntegrationEvents.Handlers;

/// <summary>
/// Handles the UserRegisteredIntegrationEvent published by Identity.API.
/// Sends a welcome email and push notification (dummy) to the newly registered user.
/// </summary>
public sealed class UserRegisteredIntegrationEventHandler(
    NotificationDispatcher dispatcher,
    ILogger<UserRegisteredIntegrationEventHandler> logger)
    : IIntegrationEventHandler<UserRegisteredIntegrationEvent>
{
    public async Task HandleAsync(UserRegisteredIntegrationEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling UserRegisteredIntegrationEvent for user {UserId} ({Email})",
            @event.UserId,
            @event.Email);

        var message = new NotificationMessage(
            Recipient: @event.Email,
            Subject: "Welcome!",
            Body: $"Hello {@event.FirstName} {@event.LastName}, welcome to eShop!"
        );

        await dispatcher.DispatchAsync(
            message,
            [NotificationChannelType.Email, NotificationChannelType.Push],
            cancellationToken);
    }
}