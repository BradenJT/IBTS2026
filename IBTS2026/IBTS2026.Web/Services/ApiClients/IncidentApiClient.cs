using System.Net;
using System.Net.Http.Json;
using IBTS2026.Web.Models;

namespace IBTS2026.Web.Services.ApiClients
{
    internal sealed class IncidentApiClient : IIncidentApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IncidentApiClient> _logger;

        public IncidentApiClient(HttpClient httpClient, ILogger<IncidentApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IncidentDetailsModel?> GetIncidentAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/incidents/{id}", ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<IncidentDetailsModel>(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incident {IncidentId}", id);
                throw;
            }
        }

        public async Task<PagedResultModel<IncidentModel>> GetIncidentsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            string? sortBy = null,
            string? sortDir = null,
            int? statusId = null,
            int? priorityId = null,
            int? assignedToUserId = null,
            DateTime? createdAfter = null,
            DateTime? createdBefore = null,
            CancellationToken ct = default)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"pageNumber={pageNumber}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");

                if (!string.IsNullOrWhiteSpace(sortBy))
                    queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

                if (!string.IsNullOrWhiteSpace(sortDir))
                    queryParams.Add($"sortDir={Uri.EscapeDataString(sortDir)}");

                if (statusId.HasValue)
                    queryParams.Add($"statusId={statusId.Value}");

                if (priorityId.HasValue)
                    queryParams.Add($"priorityId={priorityId.Value}");

                if (assignedToUserId.HasValue)
                    queryParams.Add($"assignedToUserId={assignedToUserId.Value}");

                if (createdAfter.HasValue)
                    queryParams.Add($"createdAfter={createdAfter.Value:O}");

                if (createdBefore.HasValue)
                    queryParams.Add($"createdBefore={createdBefore.Value:O}");

                var url = $"/incidents?{string.Join("&", queryParams)}";
                var result = await _httpClient.GetFromJsonAsync<PagedResultModel<IncidentModel>>(url, ct);

                return result ?? new PagedResultModel<IncidentModel>([], 0, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting incidents");
                throw;
            }
        }

        public async Task<int> CreateIncidentAsync(CreateIncidentModel model, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/incidents", model, ct);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<int>(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating incident");
                throw;
            }
        }

        public async Task<bool> UpdateIncidentAsync(int id, UpdateIncidentModel model, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/incidents/{id}", model, ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return false;

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating incident {IncidentId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteIncidentAsync(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/incidents/{id}", ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return false;

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting incident {IncidentId}", id);
                throw;
            }
        }
    }
}
