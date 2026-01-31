namespace IBTS2026.Api.Contracts.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? InvitationToken = null
);
