using IBTS2026.Domain.Entities;

namespace IBTS2026.Application.Abstractions.Services;

public interface INotificationService
{
    void QueueAssignmentNotification(Incident incident, User assignedUser);
    void QueueStatusChangeNotification(Incident incident, string oldStatus, string newStatus, User changedByUser);
    void QueuePriorityChangeNotification(Incident incident, string oldPriority, string newPriority, User changedByUser);
    void QueueNoteAddedNotification(Incident incident, User noteAuthor, User incidentCreator);
}
