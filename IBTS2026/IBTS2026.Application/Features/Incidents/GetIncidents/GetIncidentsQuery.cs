using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Incidents;
using IBTS2026.Application.Models.Requests;

namespace IBTS2026.Application.Features.Incidents.GetIncidents
{
    public sealed record GetIncidentsQuery(
        PageRequest Page,
        SortRequest? Sort,
        string? Search,
        int? StatusId,
        int? PriorityId,
        int? AssignedToUserId,
        DateTime? CreatedAfter,
        DateTime? CreatedBefore) : IQuery<PagedResult<IncidentDto>>;
}
