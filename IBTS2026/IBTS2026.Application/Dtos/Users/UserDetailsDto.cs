namespace IBTS2026.Application.Dtos.Users
{
    public sealed record UserDetailsDto(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    DateTime? CreatedAt
);

}
