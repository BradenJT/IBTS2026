namespace IBTS2026.Api.Contracts.Auth;

public sealed record LoginResponse(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string Token,
    string SecurityStamp
);
