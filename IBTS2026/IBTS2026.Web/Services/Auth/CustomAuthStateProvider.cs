using System.Security.Claims;
using IBTS2026.Web.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace IBTS2026.Web.Services.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private AuthenticationState _currentState;
    private const string UserStorageKey = "ibts_user";

    public CustomAuthStateProvider(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
        _currentState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<StoredUser>(UserStorageKey);
            if (result.Success && result.Value != null)
            {
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

    public AuthenticationState? GetCurrentAuthenticationState() => _currentState;

    public async Task LoginAsync(UserModel user)
    {
        var storedUser = new StoredUser
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role
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
    }
}
