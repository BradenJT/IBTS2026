using System.Net.Http.Headers;
using IBTS2026.Web.Services.Auth;
using Microsoft.Extensions.Logging;

namespace IBTS2026.Web.Services.Http;

public sealed class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IAuthTokenStore _tokenStore;
    private readonly ICircuitTokenCache _circuitTokenCache;
    private readonly CircuitIdProvider _circuitIdProvider;
    private readonly ILogger<AuthorizationMessageHandler> _logger;

    public AuthorizationMessageHandler(
        IAuthTokenStore tokenStore,
        ICircuitTokenCache circuitTokenCache,
        CircuitIdProvider circuitIdProvider,
        ILogger<AuthorizationMessageHandler> logger)
    {
        _tokenStore = tokenStore;
        _circuitTokenCache = circuitTokenCache;
        _circuitIdProvider = circuitIdProvider;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? token = null;

        // First try to get token from the circuit cache by CircuitId
        var circuitId = _circuitIdProvider.CircuitId;
        _logger.LogDebug("AuthorizationMessageHandler: CircuitId = {CircuitId}", circuitId ?? "(null)");

        if (!string.IsNullOrEmpty(circuitId))
        {
            token = _circuitTokenCache.GetToken(circuitId);
            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("AuthorizationMessageHandler: Token from circuit cache (by CircuitId)");
            }
        }

        // Fallback: Try to get the current token (when CircuitId is not available, e.g., in HttpClient handler scope)
        if (string.IsNullOrEmpty(token))
        {
            token = _circuitTokenCache.GetCurrentToken();
            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("AuthorizationMessageHandler: Token from circuit cache (fallback/current)");
            }
        }

        // Last resort: Try the token store (may fail during prerendering)
        if (string.IsNullOrEmpty(token))
        {
            token = await _tokenStore.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("AuthorizationMessageHandler: Token from token store");
            }
        }

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _logger.LogDebug("AuthorizationMessageHandler: Added Bearer token to request {Method} {Uri}",
                request.Method, request.RequestUri);
        }
        else
        {
            _logger.LogWarning("AuthorizationMessageHandler: No token available for request {Method} {Uri}",
                request.Method, request.RequestUri);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("AuthorizationMessageHandler: Request {Method} {Uri} returned {StatusCode}",
                request.Method, request.RequestUri, response.StatusCode);
        }

        return response;
    }
}
