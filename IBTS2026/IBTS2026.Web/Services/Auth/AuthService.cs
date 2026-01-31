using System.Security.Claims;
using IBTS2026.Web.Models;
using IBTS2026.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components.Authorization;

namespace IBTS2026.Web.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IAuthApiClient _authApiClient;
    private readonly IAuthTokenStore _tokenStore;

    public AuthService(
        AuthenticationStateProvider authStateProvider,
        IAuthApiClient authApiClient,
        IAuthTokenStore tokenStore)
    {
        _authStateProvider = authStateProvider;
        _authApiClient = authApiClient;
        _tokenStore = tokenStore;
    }

    public bool IsAuthenticated
    {
        get
        {
            var authState = (_authStateProvider as CustomAuthStateProvider)?.GetCurrentAuthenticationState();
            return authState?.User?.Identity?.IsAuthenticated ?? false;
        }
    }

    public int? CurrentUserId
    {
        get
        {
            var authState = (_authStateProvider as CustomAuthStateProvider)?.GetCurrentAuthenticationState();
            var userIdClaim = authState?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }

    public string? CurrentUserRole
    {
        get
        {
            var authState = (_authStateProvider as CustomAuthStateProvider)?.GetCurrentAuthenticationState();
            return authState?.User?.FindFirst(ClaimTypes.Role)?.Value;
        }
    }

    public async Task<UserModel?> GetCurrentUserAsync()
    {
        // Must use async method to load state from ProtectedLocalStorage after page reload
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        if (authState?.User?.Identity?.IsAuthenticated != true)
            return null;

        var userIdClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return null;

        return new UserModel(
            userId,
            authState.User.FindFirst(ClaimTypes.Email)?.Value ?? "",
            authState.User.FindFirst(ClaimTypes.GivenName)?.Value ?? "",
            authState.User.FindFirst(ClaimTypes.Surname)?.Value ?? "",
            authState.User.FindFirst(ClaimTypes.Role)?.Value ?? ""
        );
    }

    public async Task<(bool Success, string? ErrorMessage)> LoginAsync(string email, string password)
    {
        try
        {
            var result = await _authApiClient.LoginAsync(email, password);
            if (result == null)
            {
                return (false, "Invalid email or password.");
            }

            // Store the token
            await _tokenStore.SetTokenAsync(result.Token);

            // Update auth state
            if (_authStateProvider is CustomAuthStateProvider customProvider)
            {
                var user = new UserModel(
                    result.UserId,
                    result.Email,
                    result.FirstName,
                    result.LastName,
                    result.Role,
                    SecurityStamp: result.SecurityStamp
                );
                await customProvider.LoginAsync(user);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Login failed: {ex.Message}");
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string? invitationToken = null)
    {
        try
        {
            var result = await _authApiClient.RegisterAsync(email, password, firstName, lastName, invitationToken);
            if (result == null)
            {
                return (false, "Registration failed. Email may already be in use.");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Registration failed: {ex.Message}");
        }
    }

    public async Task<bool> IsFirstUserAsync()
    {
        try
        {
            return await _authApiClient.IsFirstUserAsync();
        }
        catch
        {
            return false;
        }
    }

    public async Task<InvitationInfoModel?> ValidateInvitationTokenAsync(string token)
    {
        try
        {
            return await _authApiClient.ValidateInvitationTokenAsync(token);
        }
        catch
        {
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        await _tokenStore.ClearTokenAsync();

        if (_authStateProvider is CustomAuthStateProvider customProvider)
        {
            await customProvider.LogoutAsync();
        }
    }
}
