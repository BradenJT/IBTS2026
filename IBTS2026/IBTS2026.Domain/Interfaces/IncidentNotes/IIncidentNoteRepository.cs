using IBTS2026.Domain.Entities.Features.Incidents.IncidentNote;

namespace IBTS2026.Domain.Interfaces.IncidentNotes;

public interface IIncidentNoteRepository
{
    Task<IncidentNote?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<IncidentNote>> GetByIncidentIdAsync(int incidentId, CancellationToken ct);
    void Add(IncidentNote note);
    void Remove(IncidentNote note);
}
