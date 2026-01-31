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

        // First try to get token from the circuit cache (works across scopes)
        var circuitId = _circuitIdProvider.CircuitId;
        _logger.LogDebug("AuthorizationMessageHandler: CircuitId = {CircuitId}", circuitId ?? "(null)");

        if (!string.IsNullOrEmpty(circuitId))
        {
            token = _circuitTokenCache.GetToken(circuitId);
            _logger.LogDebug("AuthorizationMessageHandler: Token from circuit cache = {HasToken}", !string.IsNullOrEmpty(token));
        }

        // Fall back to the token store if circuit cache doesn't have it
        if (string.IsNullOrEmpty(token))
        {
            token = await _tokenStore.GetTokenAsync();
            _logger.LogDebug("AuthorizationMessageHandler: Token from token store = {HasToken}", !string.IsNullOrEmpty(token));
        }

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _logger.LogInformation("AuthorizationMessageHandler: Added Bearer token to request {Method} {Uri} (token length: {Length})",
                request.Method, request.RequestUri, token.Length);
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
