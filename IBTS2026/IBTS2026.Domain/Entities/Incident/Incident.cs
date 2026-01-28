#nullable enable

using IBTS2026.Domain.Enums;

namespace IBTS2026.Domain.Entities;

public partial class Incident
{
    // Navigation properties not in the generated file
    public virtual User? CreatedByUser { get; set; }
    public virtual User? AssignedToUser { get; set; }

    public static Incident Create(
        string title,
        string description,
        int statusId,
        int priorityId,
        int createdByUserId,
        int? assignedToUserId)
    {
        return new Incident
        {
            Title = title,
            Description = description,
            StatusId = statusId,
            PriorityId = priorityId,
            CreatedBy = createdByUserId,
            AssignedTo = assignedToUserId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ChangeTitle(string title)
    {
        Title = title;
    }

    public void ChangeDescription(string description)
    {
        Description = description;
    }

    public void ChangePriority(int priorityId)
    {
        PriorityId = priorityId;
    }

    public void AssignTo(int? userId)
    {
        AssignedTo = userId;
    }

    public void ChangeStatus(int newStatusId)
    {
        // Status transition validation
        var currentStatus = (IncidentStatus)StatusId;
        var targetStatus = (IncidentStatus)newStatusId;

        if (!IsValidStatusTransition(currentStatus, targetStatus))
        {
            throw new InvalidOperationException(
                $"Invalid status transition from {currentStatus} to {targetStatus}.");
        }

        StatusId = newStatusId;
    }

    private static bool IsValidStatusTransition(IncidentStatus current, IncidentStatus target)
    {
        // Allow same status (no-op)
        if (current == target) return true;

        return current switch
        {
            IncidentStatus.Open => target is IncidentStatus.InProgress or IncidentStatus.Closed,
            IncidentStatus.InProgress => target is IncidentStatus.Open or IncidentStatus.Closed,
            IncidentStatus.Closed => target is IncidentStatus.Open, // Can reopen
            _ => false
        };
    }
}
