using System.Net.Http.Json;

namespace IBTS2026.Web.Services.ApiClients;

internal sealed class InvitationApiClient : IInvitationApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InvitationApiClient> _logger;

    public InvitationApiClient(HttpClient httpClient, ILogger<InvitationApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<InvitationModel>> GetInvitationsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/auth/invitations", ct);
            response.EnsureSuccessStatusCode();
            var invitations = await response.Content.ReadFromJsonAsync<List<InvitationModel>>(ct);
            return invitations ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching invitations");
            return [];
        }
    }

    public async Task<InvitationModel?> SendInvitationAsync(string email, string role, CancellationToken ct = default)
    {
        try
        {
            var request = new { Email = email, Role = role };
            var response = await _httpClient.PostAsJsonAsync("/auth/invite", request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Failed to send invitation to {Email}: {Error}", email, error);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<InvitationModel>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invitation to {Email}", email);
            return null;
        }
    }

    public async Task<bool> CancelInvitationAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/auth/invitations/{id}", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling invitation {Id}", id);
            return false;
        }
    }

    public async Task<bool> ResendInvitationAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/auth/invitations/{id}/resend", null, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending invitation {Id}", id);
            return false;
        }
    }
}
