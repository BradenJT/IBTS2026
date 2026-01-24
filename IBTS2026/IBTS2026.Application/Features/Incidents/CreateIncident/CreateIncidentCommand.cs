namespace IBTS2026.Application.Features.Incidents.CreateIncident
{
    public sealed record CreateIncidentCommand(
        string Title,
        string Description,
        int StatusId,
        int PriorityId,
        int CreatedByUserId,
        int? AssignedToUserId);
}
