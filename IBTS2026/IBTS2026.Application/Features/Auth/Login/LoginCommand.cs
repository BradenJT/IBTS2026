namespace IBTS2026.Application.Features.Auth.Login;

public sealed record LoginCommand(
    string Email,
    string Password
);
