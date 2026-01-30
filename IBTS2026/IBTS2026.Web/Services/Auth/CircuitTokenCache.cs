using System.Collections.Concurrent;

namespace IBTS2026.Web.Services.Auth;

/// <summary>
/// Singleton service that caches JWT tokens in memory, keyed by circuit ID.
/// This allows tokens to be shared across different DI scopes within the same Blazor circuit.
/// </summary>
public sealed class CircuitTokenCache : ICircuitTokenCache
{
    private readonly ConcurrentDictionary<string, string> _tokens = new();

    public string? GetToken(string circuitId)
    {
        return _tokens.TryGetValue(circuitId, out var token) ? token : null;
    }

    public void SetToken(string circuitId, string token)
    {
        _tokens[circuitId] = token;
    }

    public void ClearToken(string circuitId)
    {
        _tokens.TryRemove(circuitId, out _);
    }

    public void RemoveCircuit(string circuitId)
    {
        _tokens.TryRemove(circuitId, out _);
    }
}
