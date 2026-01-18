#nullable enable
namespace IBTS2026.Domain.Entities;

public partial class Incident
{
    public static Incident Create(
        string title,
        string description,
        int statusId,
        int priorityId,
        int createdByUserId,
        int? assignedToUserId,
        DateTime createdAt)
    {
        return new Incident
        {
            Title = title,
            Description = description,
            StatusId = statusId,
            PriorityId = priorityId,
            CreatedBy = createdByUserId,
            AssignedTo = assignedToUserId,
            CreatedAt = createdAt
        };
    }

    public static Incident Update(
        Incident incident,
        string title,
        string description,
        int statusId,
        int priorityId,
        int? assignedToUserId)
    {
        incident.Title = title;
        incident.Description = description;
        incident.StatusId = statusId;
        incident.PriorityId = priorityId;
        incident.AssignedTo = assignedToUserId;
        return incident;
    }
}
