namespace IBTS2026.Application.Dtos.Incidents
{
    public sealed record IncidentDto(
        int IncidentId,
        string Title,
        int StatusId,
        string StatusName,
        int PriorityId,
        string PriorityName,
        int? AssignedTo,
        DateTime CreatedAt);
}
