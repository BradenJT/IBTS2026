using IBTS2026.Web.Models;

namespace IBTS2026.Web.Services.ApiClients;

public interface IIncidentNoteApiClient
{
    Task<List<IncidentNoteModel>> GetNotesAsync(int incidentId, CancellationToken ct = default);
    Task<int> CreateNoteAsync(int incidentId, CreateIncidentNoteModel model, CancellationToken ct = default);
}
