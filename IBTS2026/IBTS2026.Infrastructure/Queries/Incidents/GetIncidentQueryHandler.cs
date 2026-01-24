using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Incidents;
using IBTS2026.Application.Features.Incidents.GetIncident;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Queries.Incidents
{
    internal sealed class GetIncidentQueryHandler(IBTS2026Context context)
        : IQueryHandler<GetIncidentQuery, IncidentDetailsDto?>
    {
        private readonly IBTS2026Context _context = context;

        public async Task<IncidentDetailsDto?> Handle(
            GetIncidentQuery query,
            CancellationToken ct)
        {
            return await _context.Incidents
                .AsNoTracking()
                .Where(i => i.IncidentId == query.IncidentId)
                .Select(i => new IncidentDetailsDto(
                    i.IncidentId,
                    i.Title,
                    i.Description,
                    i.StatusId,
                    i.Status.StatusName,
                    i.PriorityId,
                    i.Priority.PriorityName,
                    i.CreatedBy,
                    i.AssignedTo,
                    i.CreatedAt))
                .FirstOrDefaultAsync(ct);
        }
    }
}
