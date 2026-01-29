namespace IBTS2026.Api.Contracts.Auth;

public sealed record RegisterResponse(
    int UserId,
    string Email,
    string Role
);
