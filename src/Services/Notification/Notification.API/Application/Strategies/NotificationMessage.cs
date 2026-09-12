namespace Notification.API.Application.Strategies;

public sealed record NotificationMessage(
    string Recipient,
    string Subject,
    string Body
);