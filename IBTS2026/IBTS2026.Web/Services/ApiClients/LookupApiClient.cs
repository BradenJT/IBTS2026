using System.Net.Http.Json;
using IBTS2026.Web.Models;

namespace IBTS2026.Web.Services.ApiClients;

public class LookupApiClient(HttpClient httpClient) : ILookupApiClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<List<PriorityModel>> GetPrioritiesAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<PriorityModel>>("/priorities", ct);
        return result ?? [];
    }

    public async Task<List<StatusModel>> GetStatusesAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<StatusModel>>("/statuses", ct);
        return result ?? [];
    }

    public async Task<List<UserLookupModel>> GetUsersForDropdownAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<UserLookupModel>>("/users/lookup", ct);
        return result ?? [];
    }
}
