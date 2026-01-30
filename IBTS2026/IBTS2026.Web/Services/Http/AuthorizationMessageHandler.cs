using System.Net.Http.Headers;
using IBTS2026.Web.Services.Auth;

namespace IBTS2026.Web.Services.Http;

public sealed class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IAuthTokenStore _tokenStore;
    private readonly ICircuitTokenCache _circuitTokenCache;
    private readonly CircuitIdProvider _circuitIdProvider;

    public AuthorizationMessageHandler(
        IAuthTokenStore tokenStore,
        ICircuitTokenCache circuitTokenCache,
        CircuitIdProvider circuitIdProvider)
    {
        _tokenStore = tokenStore;
        _circuitTokenCache = circuitTokenCache;
        _circuitIdProvider = circuitIdProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? token = null;

        // First try to get token from the circuit cache (works across scopes)
        var circuitId = _circuitIdProvider.CircuitId;
        if (!string.IsNullOrEmpty(circuitId))
        {
            token = _circuitTokenCache.GetToken(circuitId);
        }

        // Fall back to the token store if circuit cache doesn't have it
        if (string.IsNullOrEmpty(token))
        {
            token = await _tokenStore.GetTokenAsync();
        }

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
