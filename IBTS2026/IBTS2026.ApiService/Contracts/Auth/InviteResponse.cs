namespace IBTS2026.Api.Contracts.Auth;

public sealed record InviteResponse(
    int InvitationId,
    string Email,
    string Role,
    DateTime ExpiresAt
);
