namespace IBTS2026.Api.Contracts.Users
{
    public sealed class UpdateUserRequest
    {
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
    }
}
