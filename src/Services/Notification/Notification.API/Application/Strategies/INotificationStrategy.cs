using Notification.API.Domain.Enums;

namespace Notification.API.Application.Strategies;

/// <summary>
/// Strategy interface for notification delivery channels.
/// Each implementation represents a concrete delivery mechanism (Email, SMS, Push).
/// </summary>
public interface INotificationStrategy
{
    NotificationChannelType ChannelType { get; }
    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken);
}