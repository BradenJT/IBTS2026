using IBTS2026.Domain.Entities.Features.Notifications.NotificationOutbox;

namespace IBTS2026.Domain.Interfaces.Notifications;

public interface INotificationOutboxRepository
{
    Task<IReadOnlyList<NotificationOutbox>> GetPendingNotificationsAsync(int batchSize, CancellationToken ct);
    Task<IReadOnlyList<NotificationOutbox>> GetFailedForRetryAsync(int maxRetries, int batchSize, CancellationToken ct);
    void Add(NotificationOutbox notification);
    void Update(NotificationOutbox notification);
}
