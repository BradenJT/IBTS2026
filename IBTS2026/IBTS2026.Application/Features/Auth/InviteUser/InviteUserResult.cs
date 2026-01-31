namespace IBTS2026.Application.Features.Auth.InviteUser;

public sealed record InviteUserResult(
    bool Success,
    int? InvitationId,
    string? Email,
    string? Token,
    DateTime? ExpiresAt,
    string? ErrorMessage
);
