using BuildingBlocks.Messaging.Abstractions;
using Notification.API.Application.Strategies;
using Notification.API.Domain.Enums;
using Ordering.Contracts.IntegrationEvents;

namespace Notification.API.Application.IntegrationEvents.Handlers;

/// <summary>
/// Handles the OrderPlacedIntegrationEvent published by Ordering.API.
/// Sends an email and push notification to the user about their successful order.
/// </summary>
public sealed class OrderPlacedIntegrationEventHandler(
    NotificationDispatcher dispatcher,
    ILogger<OrderPlacedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OrderPlacedIntegrationEvent>
{
    public async Task HandleAsync(OrderPlacedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling OrderPlacedIntegrationEvent for Order {OrderId}, User {UserName}",
            @event.OrderId,
            @event.UserName);

        var message = new NotificationMessage(
            Recipient: @event.UserName, // Using UserName as recipient for now (could be Email)
            Subject: "Order Successfully Placed!",
            Body: $"Dear {@event.UserName}, your order {@event.OrderId} has been placed successfully and is currently {@event.OrderStatus}."
        );

        await dispatcher.DispatchAsync(
            message,
            [NotificationChannelType.Email, NotificationChannelType.Push],
            cancellationToken);
    }
}