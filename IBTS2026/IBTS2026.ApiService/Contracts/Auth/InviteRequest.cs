namespace IBTS2026.Api.Contracts.Auth;

public sealed record InviteRequest(
    string Email,
    string Role
);
