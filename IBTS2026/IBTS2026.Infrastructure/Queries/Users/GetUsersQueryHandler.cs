using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Users;
using IBTS2026.Application.Features.Users.GetUsers;
using IBTS2026.Application.Models.Requests;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IBTS2026.Infrastructure.Queries.Users
{
    internal sealed class GetUsersQueryHandler
    : IQueryHandler<GetUsersQuery, PagedResult<UserDto>>
    {
        private readonly IBTS2026Context _context;

        public GetUsersQueryHandler(IBTS2026Context context)
        {
            _context = context;
        }

        public async Task<PagedResult<UserDto>> Handle(
            GetUsersQuery query,
            CancellationToken ct)
        {
            var users = _context.Users.AsNoTracking();

            // Filtering
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                users = users.Where(u =>
                    u.Email.Contains(query.Search) ||
                    u.FirstName.Contains(query.Search) ||
                    u.LastName.Contains(query.Search));
            }

            // Total count (before paging)
            var totalCount = await users.CountAsync(ct);

            // Sorting
            if (query.Sort is not null)
            {
                users = query.Sort.Direction == SortDirection.Asc
                    ? users.OrderBy(u => EF.Property<object>(u, query.Sort.Field))
                    : users.OrderByDescending(u => EF.Property<object>(u, query.Sort.Field));
            }

            // Paging + projection
            var items = await users
                .Skip(query.Page.Skip)
                .Take(query.Page.PageSize)
                .Select(u => new UserDto(
                    u.UserId,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.Role
                ))
                .ToListAsync(ct);

            return new PagedResult<UserDto>(
                items,
                totalCount,
                query.Page.PageNumber,
                query.Page.PageSize
            );
        }
    }
}
