using Compliance360.Application.Notifications;

namespace Compliance360.Infrastructure.Notifications;

public sealed class NoOpNotificationDispatcher : INotificationDispatcher
{
    public Task<NotificationDispatchResult> DispatchAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(NotificationDispatchResult.Sent());
    }
}
