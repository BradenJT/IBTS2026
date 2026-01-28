using System.Net.Http.Json;
using IBTS2026.Web.Models;

namespace IBTS2026.Web.Services.ApiClients;

internal sealed class IncidentNoteApiClient : IIncidentNoteApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IncidentNoteApiClient> _logger;

    public IncidentNoteApiClient(HttpClient httpClient, ILogger<IncidentNoteApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<IncidentNoteModel>> GetNotesAsync(int incidentId, CancellationToken ct = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<IncidentNoteModel>>(
                $"/incidents/{incidentId}/notes", ct);

            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notes for incident {IncidentId}", incidentId);
            throw;
        }
    }

    public async Task<int> CreateNoteAsync(int incidentId, CreateIncidentNoteModel model, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/incidents/{incidentId}/notes", model, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating note for incident {IncidentId}", incidentId);
            throw;
        }
    }
}
