namespace IBTS2026.Web.Services.Auth;

/// <summary>
/// Scoped service that provides the current circuit ID.
/// The circuit ID is set by the TokenCircuitHandler when a circuit is initialized.
/// </summary>
public sealed class CircuitIdProvider
{
    /// <summary>
    /// Gets or sets the current circuit ID.
    /// </summary>
    public string? CircuitId { get; set; }
}
