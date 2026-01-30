using Microsoft.AspNetCore.Components.Server.Circuits;

namespace IBTS2026.Web.Services.Auth;

/// <summary>
/// Circuit handler that tracks circuit lifecycle for token management.
/// Sets the circuit ID when a circuit is initialized and cleans up tokens when circuits close.
/// </summary>
public sealed class TokenCircuitHandler : CircuitHandler
{
    private readonly ICircuitTokenCache _tokenCache;
    private readonly CircuitIdProvider _circuitIdProvider;

    public TokenCircuitHandler(ICircuitTokenCache tokenCache, CircuitIdProvider circuitIdProvider)
    {
        _tokenCache = tokenCache;
        _circuitIdProvider = circuitIdProvider;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitIdProvider.CircuitId = circuit.Id;
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitIdProvider.CircuitId = circuit.Id;
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        // Clean up the token when the circuit closes
        _tokenCache.RemoveCircuit(circuit.Id);
        return Task.CompletedTask;
    }
}
