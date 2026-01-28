namespace IBTS2026.Application.Dtos.Incidents
{
    public sealed record IncidentDetailsDto(
        int IncidentId,
        string Title,
        string Description,
        int StatusId,
        string StatusName,
        int PriorityId,
        string PriorityName,
        int CreatedBy,
        string CreatedByName,
        int? AssignedTo,
        string? AssignedToName,
        DateTime CreatedAt);
}
