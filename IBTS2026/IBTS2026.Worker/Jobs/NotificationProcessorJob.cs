using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Domain.Interfaces.Notifications;
using Microsoft.Data.SqlClient;

namespace IBTS2026.Worker.Jobs;

/// <summary>
/// Background job that processes pending notifications from the outbox.
/// Uses the outbox pattern to ensure notifications are sent reliably.
/// </summary>
public sealed class NotificationProcessorJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationProcessorJob> _logger;
    private readonly TimeSpan _processInterval = TimeSpan.FromSeconds(30);
    private const int BatchSize = 20;
    private const int MaxRetries = 3;

    public NotificationProcessorJob(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationProcessorJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Processor Job started.");

        // Initial delay to allow database migrations to complete
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotificationsAsync(stoppingToken);
                await ProcessFailedNotificationsAsync(stoppingToken);
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                // Table doesn't exist yet - wait for migrations
                _logger.LogWarning("NotificationOutbox table not found. Waiting for migrations...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing notifications.");
            }

            await Task.Delay(_processInterval, stoppingToken);
        }

        _logger.LogInformation("Notification Processor Job stopped.");
    }

    private async Task ProcessPendingNotificationsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<INotificationOutboxRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pendingNotifications = await outboxRepository.GetPendingNotificationsAsync(BatchSize, ct);

        if (pendingNotifications.Count == 0)
        {
            _logger.LogDebug("No pending notifications to process.");
            return;
        }

        _logger.LogInformation("Processing {Count} pending notifications.", pendingNotifications.Count);

        foreach (var notification in pendingNotifications)
        {
            try
            {
                var success = await emailService.SendEmailAsync(
                    notification.RecipientEmail,
                    notification.Subject,
                    notification.Body,
                    ct);

                if (success)
                {
                    notification.MarkAsProcessed();
                    _logger.LogInformation(
                        "Notification {NotificationId} sent successfully to {Recipient}.",
                        notification.NotificationOutboxId,
                        notification.RecipientEmail);
                }
                else
                {
                    notification.MarkAsFailed();
                    _logger.LogWarning(
                        "Failed to send notification {NotificationId} to {Recipient}. Retry count: {RetryCount}",
                        notification.NotificationOutboxId,
                        notification.RecipientEmail,
                        notification.RetryCount);
                }

                outboxRepository.Update(notification);
            }
            catch (Exception ex)
            {
                notification.MarkAsFailed();
                outboxRepository.Update(notification);

                _logger.LogError(ex,
                    "Exception while sending notification {NotificationId} to {Recipient}.",
                    notification.NotificationOutboxId,
                    notification.RecipientEmail);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task ProcessFailedNotificationsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<INotificationOutboxRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var failedNotifications = await outboxRepository.GetFailedForRetryAsync(MaxRetries, BatchSize, ct);

        if (failedNotifications.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Retrying {Count} failed notifications.", failedNotifications.Count);

        foreach (var notification in failedNotifications)
        {
            try
            {
                notification.ResetForRetry();

                var success = await emailService.SendEmailAsync(
                    notification.RecipientEmail,
                    notification.Subject,
                    notification.Body,
                    ct);

                if (success)
                {
                    notification.MarkAsProcessed();
                    _logger.LogInformation(
                        "Notification {NotificationId} sent successfully on retry to {Recipient}.",
                        notification.NotificationOutboxId,
                        notification.RecipientEmail);
                }
                else
                {
                    notification.MarkAsFailed();
                    _logger.LogWarning(
                        "Failed to send notification {NotificationId} on retry. Retry count: {RetryCount}/{MaxRetries}",
                        notification.NotificationOutboxId,
                        notification.RetryCount,
                        MaxRetries);
                }

                outboxRepository.Update(notification);
            }
            catch (Exception ex)
            {
                notification.MarkAsFailed();
                outboxRepository.Update(notification);

                _logger.LogError(ex,
                    "Exception while retrying notification {NotificationId}.",
                    notification.NotificationOutboxId);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
