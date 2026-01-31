using System.Net;
using System.Net.Http.Json;
using IBTS2026.Web.Models.Auth;
using IBTS2026.Web.Services.Auth;

namespace IBTS2026.Web.Services.ApiClients;

internal sealed class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthApiClient> _logger;

    public AuthApiClient(HttpClient httpClient, ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<LoginResultModel?> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        try
        {
            var request = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("/auth/login", request, ct);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Login failed for {Email} - invalid credentials", email);
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<LoginResultModel>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", email);
            throw;
        }
    }

    public async Task<RegisterResultModel?> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string? invitationToken = null,
        CancellationToken ct = default)
    {
        try
        {
            var request = new
            {
                Email = email,
                Password = password,
                FirstName = firstName,
                LastName = lastName,
                InvitationToken = invitationToken
            };

            var response = await _httpClient.PostAsJsonAsync("/auth/register", request, ct);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Registration failed for {Email}: {Error}", email, error);
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RegisterResultModel>(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", email);
            throw;
        }
    }

    public async Task<bool> IsFirstUserAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/auth/check-first-user", ct);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<FirstUserCheckResult>(ct);
                return result?.IsFirstUser ?? false;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking first user status");
            return false;
        }
    }

    public async Task<InvitationInfoModel?> ValidateInvitationTokenAsync(string token, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/auth/validate-invitation?token={Uri.EscapeDataString(token)}", ct);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<InvitationInfoModel>(ct);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating invitation token");
            return null;
        }
    }

    private record FirstUserCheckResult(bool IsFirstUser);
}
