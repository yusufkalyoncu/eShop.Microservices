using Notification.API.Domain.Enums;

namespace Notification.API.Application.Strategies;

/// <summary>
/// Dummy Push notification strategy — logs the push notification instead of sending it.
/// Replace with real FCM/APNs/OneSignal client when available.
/// </summary>
public sealed class PushNotificationStrategy(ILogger<PushNotificationStrategy> logger) : INotificationStrategy
{
    public NotificationChannelType ChannelType => NotificationChannelType.Push;

    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[PUSH DUMMY] To: {Recipient} | Title: {Subject} | Body: {Body}",
            message.Recipient,
            message.Subject,
            message.Body);

        return Task.CompletedTask;
    }
}