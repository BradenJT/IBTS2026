namespace IBTS2026.Application.Features.Auth.InviteUser;

public sealed record InviteUserCommand(
    string Email,
    string Role,
    int InvitedByUserId
);
