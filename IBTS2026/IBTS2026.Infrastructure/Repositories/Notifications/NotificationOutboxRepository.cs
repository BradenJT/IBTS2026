using IBTS2026.Domain.Entities;
using IBTS2026.Domain.Interfaces.Notifications;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Repositories.Notifications;

internal sealed class NotificationOutboxRepository(IBTS2026Context context)
    : RepositoryBase<NotificationOutbox>(context.NotificationOutbox), INotificationOutboxRepository
{
    public async Task<IReadOnlyList<NotificationOutbox>> GetPendingNotificationsAsync(int batchSize, CancellationToken ct)
    {
        return await Query()
            .Where(n => n.ProcessedAt == null && n.FailedAt == null)
            .OrderBy(n => n.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<NotificationOutbox>> GetFailedForRetryAsync(int maxRetries, int batchSize, CancellationToken ct)
    {
        return await Query()
            .Where(n => n.ProcessedAt == null && n.FailedAt != null && n.RetryCount < maxRetries)
            .OrderBy(n => n.FailedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public void Add(NotificationOutbox notification) => AddEntity(notification);

    public void Update(NotificationOutbox notification) => UpdateEntity(notification);
}
