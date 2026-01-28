using System.Security.Claims;
using IBTS2026.Web.Models;
using IBTS2026.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components.Authorization;

namespace IBTS2026.Web.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IUserApiClient _userApiClient;

    public AuthService(
        AuthenticationStateProvider authStateProvider,
        IUserApiClient userApiClient)
    {
        _authStateProvider = authStateProvider;
        _userApiClient = userApiClient;
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

    public async Task<UserModel?> GetCurrentUserAsync()
    {
        var userId = CurrentUserId;
        if (userId == null)
            return null;

        try
        {
            return await _userApiClient.GetUserAsync(userId.Value);
        }
        catch
        {
            return null;
        }
    }

    public async Task LoginAsync(int userId)
    {
        var user = await _userApiClient.GetUserAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (_authStateProvider is CustomAuthStateProvider customProvider)
        {
            await customProvider.LoginAsync(user);
        }
    }

    public async Task LogoutAsync()
    {
        if (_authStateProvider is CustomAuthStateProvider customProvider)
        {
            await customProvider.LogoutAsync();
        }
    }
}
