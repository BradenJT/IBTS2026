using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Users;
using IBTS2026.Application.Models.Requests;

namespace IBTS2026.Application.Features.Users.GetUsers
{
    public sealed record GetUsersQuery(
    PageRequest Page,
    SortRequest? Sort,
    string? Search
) : IQuery<PagedResult<UserDto>>;

}
