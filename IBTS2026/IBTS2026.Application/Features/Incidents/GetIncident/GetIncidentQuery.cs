using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Incidents;

namespace IBTS2026.Application.Features.Incidents.GetIncident
{
    public sealed record GetIncidentQuery(int IncidentId) : IQuery<IncidentDetailsDto?>;
}
