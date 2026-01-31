using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Domain.Entities.Features.Incidents.Incident;
using IBTS2026.Domain.Entities.Features.Notifications.NotificationOutbox;
using IBTS2026.Domain.Entities.Features.Users;
using IBTS2026.Domain.Interfaces.Notifications;
using Microsoft.Extensions.Configuration;

namespace IBTS2026.Infrastructure.Services;

internal sealed class NotificationService : INotificationService
{
    private readonly INotificationOutboxRepository _outboxRepository;
    private readonly string _baseUrl;

    public NotificationService(
        INotificationOutboxRepository outboxRepository,
        IConfiguration configuration)
    {
        _outboxRepository = outboxRepository ?? throw new ArgumentNullException(nameof(outboxRepository));
        _baseUrl = configuration["AppSettings:BaseUrl"] ?? "https://localhost:7001";
    }

    public void QueueAssignmentNotification(Incident incident, User assignedUser)
    {
        var incidentLink = $"{_baseUrl}/incidents/{incident.IncidentId}";
        var subject = $"You have been assigned to incident: {incident.Title}";
        var body = $@"
            <h2>Incident Assignment</h2>
            <p>You have been assigned to the following incident:</p>
            <table style='border-collapse: collapse; margin: 16px 0;'>
                <tr><td style='padding: 8px; border: 1px solid #ddd;'><strong>Title:</strong></td><td style='padding: 8px; border: 1px solid #ddd;'>{incident.Title}</td></tr>
                <tr><td style='padding: 8px; border: 1px solid #ddd;'><strong>Description:</strong></td><td style='padding: 8px; border: 1px solid #ddd;'>{incident.Description}</td></tr>
            </table>
            <p><a href='{incidentLink}' style='color: #5E81AC;'>View Incident</a></p>
        ";

        var notification = NotificationOutbox.Create(
            "Assignment",
            assignedUser.Email,
            subject,
            body,
            incident.IncidentId);

        _outboxRepository.Add(notification);
    }

    public void QueueStatusChangeNotification(Incident incident, string oldStatus, string newStatus, User changedByUser)
    {
        var incidentLink = $"{_baseUrl}/incidents/{incident.IncidentId}";
        var subject = $"Incident status changed: {incident.Title}";
        var body = $@"
            <h2>Incident Status Changed</h2>
            <p>The status of the following incident has been updated:</p>
            <table style='border-collapse: collapse; margin: 16px 0;'>
                <tr><td style='padding: 8px; border: 1px solid #ddd;'><strong>Title:</strong></td><td style='padding: 8px; border: 1px solid #ddd;'>{incident.Title}</td></tr>
                <tr><td style='padding: 8px; border: 1px solid #ddd;'><strong>Old Status:</strong></td><td style='padding: 8px; border: 1px solid #ddd;'>{oldStatus}</td></tr>
                <tr><td style='padding: 8px; border: 1px solid #ddd;'><strong>New Status:</strong></td><td style='padding: 8px; border: 1px solid #ddd;'>{newStatus}</td></tr>
                <tr><td style='padding: 8px; border: 1px solid #ddd;'><strong>Changed By:</strong></td><td style='padding: 8px; border: 1px solid #ddd;'>{changedByUser.FirstName} {changedByUser.LastName}</td></tr>
            </table>
            <p><a href='{incidentLink}' style='color: #5E81AC;'>View Incident</a></p>
        ";

        // Notify the incident creator if they exist and have an email
        if (incident.CreatedByUser != null && !string.IsNullOrEmpty(incident.CreatedByUser.Email))
        {
            var notification = NotificationOutbox.Create(
                "StatusChange",
                incident.CreatedByUser.Email,
                subject,
                body,
                incident.IncidentId);

            _outboxRepository.Add(notification);
        }
    }

    public void QueuePriorityChangeNotification(Incident incident, string oldPriority, string newPriority, User changedByUser)
    {
        var incidentLink = $"{_baseUrl}/incidents/{incident.IncidentId}";
        var subject = $"Incident priority changed: {incident.Title}";
        var body = $@"
            <h2>Incident Priority Changed</h2>
            <p>The priority of the following incident has been updated:</p>
            <table style='border-collapse: collapse; margin: 16px 0;'>
                <tr><td style='padding: 8px; border: 1px solid #ddd;'><strong>Title:</strong></td><td style='padding: 8px; border: 1px solid #ddd;'>{incident.Title}</td></tr>
                <tr><td style='padding: 8px; border: 1px solid #ddd;'><strong>Old Priority:</strong></td><td style='padding: 8px; border: 1px solid #ddd;'>{oldPriority}</td></tr>
                <tr><td style='padding: 8px; border: 1px solid #ddd;'><strong>New Priority:</strong></td><td style='padding: 8px; border: 1px solid #ddd;'>{newPriority}</td></tr>
                <tr><td style='padding: 8px; border: 1px solid #ddd;'><strong>Changed By:</strong></td><td style='padding: 8px; border: 1px solid #ddd;'>{changedByUser.FirstName} {changedByUser.LastName}</td></tr>
            </table>
            <p><a href='{incidentLink}' style='color: #5E81AC;'>View Incident</a></p>
        ";

        // Notify the incident creator if they exist and have an email
        if (incident.CreatedByUser != null && !string.IsNullOrEmpty(incident.CreatedByUser.Email))
        {
            var notification = NotificationOutbox.Create(
                "PriorityChange",
                incident.CreatedByUser.Email,
                subject,
                body,
                incident.IncidentId);

            _outboxRepository.Add(notification);
        }
    }

    public void QueueNoteAddedNotification(Incident incident, User noteAuthor, User incidentCreator)
    {
        // Don't notify if the note author is the incident creator
        if (noteAuthor.UserId == incidentCreator.UserId)
            return;

        var incidentLink = $"{_baseUrl}/incidents/{incident.IncidentId}";
        var subject = $"New note added to incident: {incident.Title}";
        var body = $@"
            <h2>New Note Added</h2>
            <p>A new note has been added to your incident:</p>
            <table style='border-collapse: collapse; margin: 16px 0;'>
                <tr><td style='padding: 8px; border: 1px solid #ddd;'><strong>Title:</strong></td><td style='padding: 8px; border: 1px solid #ddd;'>{incident.Title}</td></tr>
                <tr><td style='padding: 8px; border: 1px solid #ddd;'><strong>Note Added By:</strong></td><td style='padding: 8px; border: 1px solid #ddd;'>{noteAuthor.FirstName} {noteAuthor.LastName}</td></tr>
            </table>
            <p><a href='{incidentLink}' style='color: #5E81AC;'>View Incident</a></p>
        ";

        if (!string.IsNullOrEmpty(incidentCreator.Email))
        {
            var notification = NotificationOutbox.Create(
                "NoteAdded",
                incidentCreator.Email,
                subject,
                body,
                incident.IncidentId);

            _outboxRepository.Add(notification);
        }
    }

    public void QueueInvitationNotification(string recipientEmail, string inviterFirstName, string inviterLastName, string role, string invitationToken, DateTime expiresAt)
    {
        var registrationUrl = $"{_baseUrl}/register?token={invitationToken}";
        var subject = "You've been invited to join IBTS2026";
        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset=""utf-8"">
                <title>IBTS2026 Invitation</title>
            </head>
            <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
                <div style=""max-width: 600px; margin: 0 auto; padding: 20px;"">
                    <h1 style=""color: #2c3e50;"">You've Been Invited!</h1>

                    <p>Hello,</p>

                    <p><strong>{inviterFirstName} {inviterLastName}</strong> has invited you to join IBTS2026 as a <strong>{role}</strong>.</p>

                    <p>Click the button below to complete your registration:</p>

                    <div style=""text-align: center; margin: 30px 0;"">
                        <a href=""{registrationUrl}""
                           style=""background-color: #3498db; color: white; padding: 12px 24px;
                                  text-decoration: none; border-radius: 5px; display: inline-block;"">
                            Accept Invitation
                        </a>
                    </div>

                    <p style=""color: #7f8c8d; font-size: 14px;"">
                        This invitation will expire on <strong>{expiresAt:MMMM dd, yyyy 'at' h:mm tt} UTC</strong>.
                    </p>

                    <p style=""color: #7f8c8d; font-size: 14px;"">
                        If you didn't expect this invitation or believe it was sent in error,
                        you can safely ignore this email.
                    </p>

                    <hr style=""border: none; border-top: 1px solid #eee; margin: 30px 0;"">

                    <p style=""color: #95a5a6; font-size: 12px;"">
                        If the button doesn't work, copy and paste this link into your browser:<br>
                        <a href=""{registrationUrl}"" style=""color: #3498db;"">{registrationUrl}</a>
                    </p>
                </div>
            </body>
            </html>
        ";

        var notification = NotificationOutbox.Create(
            "Invitation",
            recipientEmail,
            subject,
            body,
            relatedIncidentId: null);

        _outboxRepository.Add(notification);
    }
}
