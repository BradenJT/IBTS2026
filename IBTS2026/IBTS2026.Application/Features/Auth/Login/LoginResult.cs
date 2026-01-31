namespace IBTS2026.Application.Features.Auth.Login;

public sealed record LoginResult(
    bool Success,
    int? UserId,
    string? Email,
    string? FirstName,
    string? LastName,
    string? Role,
    string? Token,
    string? SecurityStamp,
    string? ErrorMessage
);
