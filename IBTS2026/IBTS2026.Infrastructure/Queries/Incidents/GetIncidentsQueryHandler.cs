using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Incidents;
using IBTS2026.Application.Features.Incidents.GetIncidents;
using IBTS2026.Application.Models.Requests;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Queries.Incidents
{
    internal sealed class GetIncidentsQueryHandler(IBTS2026Context context)
        : IQueryHandler<GetIncidentsQuery, PagedResult<IncidentDto>>
    {
        private readonly IBTS2026Context _context = context;

        public async Task<PagedResult<IncidentDto>> Handle(
            GetIncidentsQuery query,
            CancellationToken ct)
        {
            var incidents = _context.Incidents.AsNoTracking();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                incidents = incidents.Where(i =>
                    i.Title.Contains(query.Search) ||
                    i.Description.Contains(query.Search));
            }

            if (query.StatusId.HasValue)
            {
                incidents = incidents.Where(i => i.StatusId == query.StatusId.Value);
            }

            if (query.PriorityId.HasValue)
            {
                incidents = incidents.Where(i => i.PriorityId == query.PriorityId.Value);
            }

            if (query.AssignedToUserId.HasValue)
            {
                incidents = incidents.Where(i => i.AssignedTo == query.AssignedToUserId.Value);
            }

            if (query.CreatedAfter.HasValue)
            {
                incidents = incidents.Where(i => i.CreatedAt >= query.CreatedAfter.Value);
            }

            if (query.CreatedBefore.HasValue)
            {
                incidents = incidents.Where(i => i.CreatedAt <= query.CreatedBefore.Value);
            }

            // Get total count before paging
            var totalCount = await incidents.CountAsync(ct);

            // Apply sorting
            if (query.Sort is not null)
            {
                incidents = query.Sort.Direction == SortDirection.Asc
                    ? incidents.OrderBy(i => EF.Property<object>(i, query.Sort.Field))
                    : incidents.OrderByDescending(i => EF.Property<object>(i, query.Sort.Field));
            }
            else
            {
                // Default sort by CreatedAt descending
                incidents = incidents.OrderByDescending(i => i.CreatedAt);
            }

            // Apply paging and projection
            var items = await incidents
                .Skip(query.Page.Skip)
                .Take(query.Page.PageSize)
                .Select(i => new IncidentDto(
                    i.IncidentId,
                    i.Title,
                    i.StatusId,
                    i.Status.StatusName,
                    i.PriorityId,
                    i.Priority.PriorityName,
                    i.AssignedTo,
                    i.CreatedAt))
                .ToListAsync(ct);

            return new PagedResult<IncidentDto>(
                items,
                totalCount,
                query.Page.PageNumber,
                query.Page.PageSize);
        }
    }
}
