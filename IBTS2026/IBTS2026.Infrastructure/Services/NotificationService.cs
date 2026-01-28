using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Domain.Entities;
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
}
