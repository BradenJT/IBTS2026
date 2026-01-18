namespace IBTS2026.Application.Features.Users.CreateUser
{
    public sealed record CreateUserCommand(
        string Email,
        string FirstName,
        string LastName,
        string Role
        );
}
