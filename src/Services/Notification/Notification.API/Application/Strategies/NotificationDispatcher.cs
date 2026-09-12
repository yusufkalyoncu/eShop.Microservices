using Notification.API.Domain.Enums;

namespace Notification.API.Application.Strategies;

/// <summary>
/// Context class in Strategy pattern.
/// Resolves the appropriate INotificationStrategy implementations and dispatches the message.
/// </summary>
public sealed class NotificationDispatcher(IEnumerable<INotificationStrategy> strategies)
{
    public async Task DispatchAsync(
        NotificationMessage message,
        IEnumerable<NotificationChannelType> targetChannels,
        CancellationToken cancellationToken)
    {
        foreach (var channelType in targetChannels)
        {
            var strategy = strategies.FirstOrDefault(s => s.ChannelType == channelType);
            if (strategy is not null)
            {
                await strategy.SendAsync(message, cancellationToken);
            }
        }
    }
}