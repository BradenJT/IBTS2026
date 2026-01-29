namespace IBTS2026.Api.Contracts.Auth;

public sealed record LoginRequest(
    string Email,
    string Password
);
