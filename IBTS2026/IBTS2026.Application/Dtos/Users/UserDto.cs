namespace IBTS2026.Application.Dtos.Users
{
    public sealed record UserDto(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role
);

}
