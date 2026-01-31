namespace IBTS2026.Application.Features.Users.UpdateUser;

public sealed record UpdateUserCommand(
    int UserId,
    string? Email,
    string? FirstName,
    string? LastName,
    string? Role,
    string? NewPassword,
    int CurrentUserId,
    string CurrentUserRole
);

