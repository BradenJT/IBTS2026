using System.Collections.Concurrent;

namespace IBTS2026.Web.Services.Auth;

/// <summary>
/// Singleton service that caches JWT tokens in memory, keyed by circuit ID.
/// This allows tokens to be shared across different DI scopes within the same Blazor circuit.
/// Also maintains a "current" token for use by HttpClient handlers that don't have access to CircuitId.
/// </summary>
public sealed class CircuitTokenCache : ICircuitTokenCache
{
    private readonly ConcurrentDictionary<string, string> _tokens = new();
    private volatile string? _currentToken;

    public string? GetToken(string circuitId)
    {
        return _tokens.TryGetValue(circuitId, out var token) ? token : null;
    }

    public string? GetCurrentToken()
    {
        return _currentToken;
    }

    public void SetToken(string circuitId, string token)
    {
        _tokens[circuitId] = token;
        // Also set as current token for fallback access
        _currentToken = token;
    }

    public void ClearToken(string circuitId)
    {
        _tokens.TryRemove(circuitId, out _);
    }

    public void ClearCurrentToken()
    {
        _currentToken = null;
    }

    public void RemoveCircuit(string circuitId)
    {
        if (_tokens.TryRemove(circuitId, out var removedToken))
        {
            // If this was the current token, clear it
            if (_currentToken == removedToken)
            {
                _currentToken = null;
            }
        }
    }
}
