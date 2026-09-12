using Notification.API.Domain.Enums;

namespace Notification.API.Application.Strategies;

/// <summary>
/// Dummy Email strategy — logs the email instead of sending it.
/// Replace with real SMTP/SendGrid/SES client when available.
/// </summary>
public sealed class EmailNotificationStrategy(ILogger<EmailNotificationStrategy> logger) : INotificationStrategy
{
    public NotificationChannelType ChannelType => NotificationChannelType.Email;

    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[EMAIL DUMMY] To: {Recipient} | Subject: {Subject} | Body: {Body}",
            message.Recipient,
            message.Subject,
            message.Body);

        return Task.CompletedTask;
    }
}