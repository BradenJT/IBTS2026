using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace IBTS2026.Web.Services.Auth;

public sealed class AuthTokenStore : IAuthTokenStore
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ICircuitTokenCache _circuitTokenCache;
    private readonly CircuitIdProvider _circuitIdProvider;
    private readonly ILogger<AuthTokenStore> _logger;
    private const string TokenKey = "ibts_auth_token";

    private string? _cachedToken;

    public AuthTokenStore(
        ProtectedLocalStorage localStorage,
        ICircuitTokenCache circuitTokenCache,
        CircuitIdProvider circuitIdProvider,
        ILogger<AuthTokenStore> logger)
    {
        _localStorage = localStorage;
        _circuitTokenCache = circuitTokenCache;
        _circuitIdProvider = circuitIdProvider;
        _logger = logger;
    }

    public async Task<string?> GetTokenAsync()
    {
        // First try the in-memory cache (fastest)
        if (_cachedToken is not null)
        {
            _logger.LogDebug("Token retrieved from in-memory cache");
            return _cachedToken;
        }

        // Then try the circuit token cache (works across scopes)
        var circuitId = _circuitIdProvider.CircuitId;
        _logger.LogDebug("Attempting to get token. CircuitId: {CircuitId}", circuitId ?? "(null)");

        if (!string.IsNullOrEmpty(circuitId))
        {
            var circuitToken = _circuitTokenCache.GetToken(circuitId);
            if (!string.IsNullOrEmpty(circuitToken))
            {
                _cachedToken = circuitToken;
                _logger.LogDebug("Token retrieved from circuit cache for CircuitId: {CircuitId}", circuitId);
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
                _logger.LogDebug("Token retrieved from ProtectedLocalStorage");

                // Also update the circuit cache for cross-scope access
                if (!string.IsNullOrEmpty(circuitId))
                {
                    _circuitTokenCache.SetToken(circuitId, _cachedToken);
                    _logger.LogDebug("Token stored in circuit cache for CircuitId: {CircuitId}", circuitId);
                }

                return _cachedToken;
            }
            else
            {
                _logger.LogDebug("No token found in ProtectedLocalStorage (Success: {Success})", result.Success);
            }
        }
        catch (Exception ex)
        {
            // Storage not available (e.g., during prerendering)
            _logger.LogWarning(ex, "Failed to retrieve token from ProtectedLocalStorage (may be during prerendering)");
        }

        _logger.LogDebug("No token available from any source");
        return null;
    }

    public async Task SetTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Attempted to set null or empty token - ignoring");
            return;
        }

        _logger.LogInformation("Setting authentication token (length: {Length})", token.Length);
        _cachedToken = token;

        // Update the circuit cache for cross-scope access
        var circuitId = _circuitIdProvider.CircuitId;
        if (!string.IsNullOrEmpty(circuitId))
        {
            _circuitTokenCache.SetToken(circuitId, token);
            _logger.LogDebug("Token stored in circuit cache for CircuitId: {CircuitId}", circuitId);
        }
        else
        {
            _logger.LogWarning("CircuitId is null - token only stored in memory, may be lost on circuit reconnect");
        }

        try
        {
            await _localStorage.SetAsync(TokenKey, token);
            _logger.LogDebug("Token persisted to ProtectedLocalStorage");
        }
        catch (Exception ex)
        {
            // Storage not available
            _logger.LogWarning(ex, "Failed to persist token to ProtectedLocalStorage");
        }
    }

    public async Task ClearTokenAsync()
    {
        _logger.LogInformation("Clearing authentication token");
        _cachedToken = null;

        // Clear from circuit cache
        var circuitId = _circuitIdProvider.CircuitId;
        if (!string.IsNullOrEmpty(circuitId))
        {
            _circuitTokenCache.ClearToken(circuitId);
            _logger.LogDebug("Token cleared from circuit cache for CircuitId: {CircuitId}", circuitId);
        }

        // Also clear the fallback/current token
        _circuitTokenCache.ClearCurrentToken();
        _logger.LogDebug("Current/fallback token cleared");

        try
        {
            await _localStorage.DeleteAsync(TokenKey);
            _logger.LogDebug("Token cleared from ProtectedLocalStorage");
        }
        catch (Exception ex)
        {
            // Storage not available
            _logger.LogWarning(ex, "Failed to clear token from ProtectedLocalStorage");
        }
    }
}
