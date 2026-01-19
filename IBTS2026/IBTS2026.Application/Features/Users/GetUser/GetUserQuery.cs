using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Users;

namespace IBTS2026.Application.Features.Users.GetUser
{
    public sealed record GetUserQuery(int UserId)
    : IQuery<UserDetailsDto>;
}
