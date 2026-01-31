namespace IBTS2026.Application.Features.Auth.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? InvitationToken = null
);
