namespace IBTS2026.Application.Features.Incidents.UpdateIncident
{
    public sealed record UpdateIncidentCommand(
        int IncidentId,
        string Title,
        string Description,
        int StatusId,
        int PriorityId,
        int CreatedByUserId,
        int? AssignedToUserId
        );
}
