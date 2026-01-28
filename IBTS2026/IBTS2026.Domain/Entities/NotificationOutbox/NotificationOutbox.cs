#nullable enable

namespace IBTS2026.Domain.Entities;

public class NotificationOutbox
{
    public int NotificationOutboxId { get; set; }
    public string NotificationType { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int? RelatedIncidentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public int RetryCount { get; set; }

    public static NotificationOutbox Create(
        string notificationType,
        string recipientEmail,
        string subject,
        string body,
        int? relatedIncidentId = null)
    {
        return new NotificationOutbox
        {
            NotificationType = notificationType,
            RecipientEmail = recipientEmail,
            Subject = subject,
            Body = body,
            RelatedIncidentId = relatedIncidentId,
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0
        };
    }

    public void MarkAsProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        FailedAt = DateTime.UtcNow;
        RetryCount++;
    }

    public void ResetForRetry()
    {
        FailedAt = null;
    }
}
