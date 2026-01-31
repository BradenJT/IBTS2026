namespace IBTS2026.Api.Contracts.Users;

public sealed class UpdateUserRequest
{
    public string? Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Role { get; init; }
    public string? NewPassword { get; init; }
}

