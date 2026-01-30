using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace IBTS2026.Web.Services.Auth;

public sealed class AuthTokenStore : IAuthTokenStore
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ICircuitTokenCache _circuitTokenCache;
    private readonly CircuitIdProvider _circuitIdProvider;
    private const string TokenKey = "ibts_auth_token";

    private string? _cachedToken;

    public AuthTokenStore(
        ProtectedLocalStorage localStorage,
        ICircuitTokenCache circuitTokenCache,
        CircuitIdProvider circuitIdProvider)
    {
        _localStorage = localStorage;
        _circuitTokenCache = circuitTokenCache;
        _circuitIdProvider = circuitIdProvider;
    }

    public async Task<string?> GetTokenAsync()
    {
        // First try the in-memory cache (fastest)
        if (_cachedToken is not null)
            return _cachedToken;

        // Then try the circuit token cache (works across scopes)
        var circuitId = _circuitIdProvider.CircuitId;
        if (!string.IsNullOrEmpty(circuitId))
        {
            var circuitToken = _circuitTokenCache.GetToken(circuitId);
            if (!string.IsNullOrEmpty(circuitToken))
            {
                _cachedToken = circuitToken;
                return _cachedToken;
            }
        }

        // Finally try ProtectedLocalStorage (works after JS interop is available)
        try
        {
            var result = await _localStorage.GetAsync<string>(TokenKey);
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                _cachedToken = result.Value;

                // Also update the circuit cache for cross-scope access
                if (!string.IsNullOrEmpty(circuitId))
                {
                    _circuitTokenCache.SetToken(circuitId, _cachedToken);
                }

                return _cachedToken;
            }
        }
        catch
        {
            // Storage not available (e.g., during prerendering)
        }

        return null;
    }

    public async Task SetTokenAsync(string token)
    {
        _cachedToken = token;

        // Update the circuit cache for cross-scope access
        var circuitId = _circuitIdProvider.CircuitId;
        if (!string.IsNullOrEmpty(circuitId))
        {
            _circuitTokenCache.SetToken(circuitId, token);
        }

        try
        {
            await _localStorage.SetAsync(TokenKey, token);
        }
        catch
        {
            // Storage not available
        }
    }

    public async Task ClearTokenAsync()
    {
        _cachedToken = null;

        // Clear from circuit cache
        var circuitId = _circuitIdProvider.CircuitId;
        if (!string.IsNullOrEmpty(circuitId))
        {
            _circuitTokenCache.ClearToken(circuitId);
        }

        try
        {
            await _localStorage.DeleteAsync(TokenKey);
        }
        catch
        {
            // Storage not available
        }
    }
}
