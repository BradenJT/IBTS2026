using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.IncidentNotes;
using IBTS2026.Application.Features.IncidentNotes.GetIncidentNotes;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Queries.IncidentNotes;

internal sealed class GetIncidentNotesQueryHandler(IBTS2026Context context)
    : IQueryHandler<GetIncidentNotesQuery, IReadOnlyList<IncidentNoteDto>>
{
    private readonly IBTS2026Context _context = context;

    public async Task<IReadOnlyList<IncidentNoteDto>> Handle(
        GetIncidentNotesQuery query,
        CancellationToken ct)
    {
        return await _context.IncidentNotes
            .AsNoTracking()
            .Where(n => n.IncidentId == query.IncidentId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new IncidentNoteDto(
                n.IncidentNoteId,
                n.IncidentId,
                n.CreatedByUserId,
                _context.Users
                    .Where(u => u.UserId == n.CreatedByUserId)
                    .Select(u => u.FirstName + " " + u.LastName)
                    .FirstOrDefault() ?? "Unknown",
                n.Content,
                n.CreatedAt))
            .ToListAsync(ct);
    }
}
