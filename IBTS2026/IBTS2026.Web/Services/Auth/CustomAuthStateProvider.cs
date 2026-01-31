using System.Security.Claims;
using IBTS2026.Web.Models;
using IBTS2026.Web.Services.ApiClients;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace IBTS2026.Web.Services.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly IAuthApiClient _authApiClient;
    private readonly ILogger<CustomAuthStateProvider> _logger;
    private AuthenticationState _currentState;
    private const string UserStorageKey = "ibts_user";
    private DateTime _lastValidation = DateTime.MinValue;
    private static readonly TimeSpan ValidationInterval = TimeSpan.FromMinutes(5);

    public CustomAuthStateProvider(
        ProtectedLocalStorage localStorage,
        IAuthApiClient authApiClient,
        ILogger<CustomAuthStateProvider> logger)
    {
        _localStorage = localStorage;
        _authApiClient = authApiClient;
        _logger = logger;
        _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<StoredUser>(UserStorageKey);
            if (result.Success && result.Value != null)
            {
                // Periodically validate the security stamp
                if (ShouldValidateSecurityStamp())
                {
                    var isValid = await ValidateSecurityStampAsync(result.Value);
                    if (!isValid)
                    {
                        _logger.LogWarning("Security stamp validation failed for user {UserId}, logging out", result.Value.UserId);
                        await LogoutAsync();
                        return _currentState;
                    }
                }

                var claims = CreateClaims(result.Value);
                var identity = new ClaimsIdentity(claims, "custom");
                _currentState = new AuthenticationState(new ClaimsPrincipal(identity));
            }
            else
            {
                _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
        catch
        {
            // Storage not available (e.g., during prerendering)
            _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        return _currentState;
    }

    private bool ShouldValidateSecurityStamp()
    {
        return DateTime.UtcNow - _lastValidation > ValidationInterval;
    }

    private async Task<bool> ValidateSecurityStampAsync(StoredUser user)
    {
        if (string.IsNullOrEmpty(user.SecurityStamp))
        {
            // No security stamp stored - legacy session, force re-login
            _logger.LogInformation("No security stamp found for user {UserId}, requiring re-login", user.UserId);
            return false;
        }

        try
        {
            _lastValidation = DateTime.UtcNow;
            return await _authApiClient.ValidateSecurityStampAsync(user.UserId, user.SecurityStamp);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate security stamp for user {UserId}, allowing session to continue", user.UserId);
            // On network errors, allow the session to continue
            return true;
        }
    }

    /// <summary>
    /// Forces immediate validation of the security stamp, regardless of the validation interval.
    /// Returns true if valid, false if invalid (user will be logged out).
    /// </summary>
    public async Task<bool> ForceValidateSecurityStampAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<StoredUser>(UserStorageKey);
            if (!result.Success || result.Value == null)
            {
                return false;
            }

            _lastValidation = DateTime.MinValue; // Reset to force validation
            var isValid = await ValidateSecurityStampAsync(result.Value);
            if (!isValid)
            {
                await LogoutAsync();
            }
            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public AuthenticationState? GetCurrentAuthenticationState() => _currentState;

    public async Task LoginAsync(UserModel user)
    {
        var storedUser = new StoredUser
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            SecurityStamp = user.SecurityStamp ?? string.Empty
        };

        await _localStorage.SetAsync(UserStorageKey, storedUser);

        var claims = CreateClaims(storedUser);
        var identity = new ClaimsIdentity(claims, "custom");
        _currentState = new AuthenticationState(new ClaimsPrincipal(identity));

        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    public async Task LogoutAsync()
    {
        await _localStorage.DeleteAsync(UserStorageKey);

        _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        NotifyAuthenticationStateChanged(Task.FromResult(_currentState));
    }

    private static IEnumerable<Claim> CreateClaims(StoredUser user)
    {
        return new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role)
        };
    }

    private class StoredUser
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string SecurityStamp { get; set; } = string.Empty;
    }
}
