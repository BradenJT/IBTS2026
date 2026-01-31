namespace IBTS2026.Application.Features.Auth.RegisterUser;

public sealed record RegisterUserResult(
    bool Success,
    int? UserId,
    string? Email,
    string? Role,
    string? ErrorMessage
);
