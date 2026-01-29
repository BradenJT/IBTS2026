using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace IBTS2026.Web.Services.Auth;

public sealed class AuthTokenStore : IAuthTokenStore
{
    private readonly ProtectedLocalStorage _localStorage;
    private const string TokenKey = "ibts_auth_token";

    private string? _cachedToken;

    public AuthTokenStore(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task<string?> GetTokenAsync()
    {
        if (_cachedToken is not null)
            return _cachedToken;

        try
        {
            var result = await _localStorage.GetAsync<string>(TokenKey);
            if (result.Success && !string.IsNullOrEmpty(result.Value))
            {
                _cachedToken = result.Value;
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
