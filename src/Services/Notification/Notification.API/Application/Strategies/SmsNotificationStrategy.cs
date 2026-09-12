using Notification.API.Domain.Enums;

namespace Notification.API.Application.Strategies;

/// <summary>
/// Dummy SMS strategy — logs the SMS instead of sending it.
/// Replace with real Twilio/Vonage/Netgsm client when available.
/// </summary>
public sealed class SmsNotificationStrategy(ILogger<SmsNotificationStrategy> logger) : INotificationStrategy
{
    public NotificationChannelType ChannelType => NotificationChannelType.Sms;

    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[SMS DUMMY] To: {Recipient} | Message: {Body}",
            message.Recipient,
            message.Body);

        return Task.CompletedTask;
    }
}