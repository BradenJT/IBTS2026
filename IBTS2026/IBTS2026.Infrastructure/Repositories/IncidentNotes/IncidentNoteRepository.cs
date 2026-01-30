using IBTS2026.Domain.Entities.Features.Incidents.IncidentNote;
using IBTS2026.Domain.Interfaces.IncidentNotes;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Repositories.IncidentNotes;

internal sealed class IncidentNoteRepository(IBTS2026Context context)
    : RepositoryBase<IncidentNote>(context.IncidentNotes), IIncidentNoteRepository
{
    public Task<IncidentNote?> GetByIdAsync(int id, CancellationToken ct)
        => Query().FirstOrDefaultAsync(n => n.IncidentNoteId == id, ct);

    public async Task<IReadOnlyList<IncidentNote>> GetByIncidentIdAsync(int incidentId, CancellationToken ct)
    {
        return await Query()
            .Where(n => n.IncidentId == incidentId)
            .Include(n => n.CreatedByUser)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public void Add(IncidentNote note) => AddEntity(note);

    public void Remove(IncidentNote note) => RemoveEntity(note);
}
