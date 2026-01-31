namespace IBTS2026.Api.Contracts.Auth;

public sealed record ValidateSecurityStampRequest(
    int UserId,
    string SecurityStamp
);
