using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Users;
using IBTS2026.Application.Features.Users.GetUser;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IBTS2026.Infrastructure.Queries.Users
{
    internal sealed class GetUserQueryHandler(IBTS2026Context context)
        : IQueryHandler<GetUserQuery, UserDetailsDto?>
    {
        private readonly IBTS2026Context _context = context;

        public async Task<UserDetailsDto?> Handle(
            GetUserQuery query,
            CancellationToken ct)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == query.UserId)
                .Select(u => new UserDetailsDto(
                    u.UserId,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.Role,
                    u.CreatedAt
                ))
                .FirstOrDefaultAsync(ct);
        }
    }
}
