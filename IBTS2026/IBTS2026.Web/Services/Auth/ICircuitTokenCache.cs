namespace IBTS2026.Web.Services.Auth;

/// <summary>
/// Provides in-memory token caching for the current circuit.
/// This is used to share tokens between different DI scopes within the same Blazor circuit.
/// </summary>
public interface ICircuitTokenCache
{
    /// <summary>
    /// Gets the cached token for the specified circuit.
    /// </summary>
    string? GetToken(string circuitId);

    /// <summary>
    /// Sets the token for the specified circuit.
    /// </summary>
    void SetToken(string circuitId, string token);

    /// <summary>
    /// Clears the token for the specified circuit.
    /// </summary>
    void ClearToken(string circuitId);

    /// <summary>
    /// Removes all tokens associated with a circuit (cleanup on disconnect).
    /// </summary>
    void RemoveCircuit(string circuitId);
}
