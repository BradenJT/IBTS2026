using IBTS2026.Api.Contracts.Users;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Users;
using IBTS2026.Application.Features.Users.CreateUser;
using IBTS2026.Application.Features.Users.GetUser;
using IBTS2026.Application.Features.Users.GetUsers;
using IBTS2026.Application.Features.Users.RemoveUser;
using IBTS2026.Application.Features.Users.UpdateUser;
using IBTS2026.Application.Models.Requests;
using IBTS2026.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IBTS2026.ApiService.Controllers.Users
{
    [ApiController]
    [Route("users")]
    [Authorize(Policy = "RequireUserRole")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly IRequestDispatcher _dispatcher;
        private readonly IBTS2026Context _context;

        public UsersController(
            IRequestDispatcher dispatcher,
            IBTS2026Context context)
        {
            _dispatcher = dispatcher;
            _context = context;
        }

        /// <summary>
        /// Get a user by ID
        /// </summary>
        /// <remarks>
        /// Retrieves detailed information about a specific user by their unique identifier.
        /// </remarks>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUser(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _dispatcher
                .SendAsync<GetUserQuery, UserDetailsDto?>(
                    new GetUserQuery(id), ct);

            return result is null
                ? NotFound()
                : Ok(result);
        }

        /// <summary>
        /// Get a paginated list of users
        /// </summary>
        /// <remarks>
        /// Retrieves a paginated list of users with optional search and sorting. 
        /// Defaults to page 1 with 20 items per page. Requires Admin role.
        /// </remarks>
        [HttpGet]
        [Authorize(Policy = "RequireAdminRole")]
        [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize,
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortDir,
            CancellationToken ct)
        {
            var query = new GetUsersQuery(
                new PageRequest(pageNumber ?? 1, pageSize ?? 20),
                sortBy is null
                    ? null
                    : new SortRequest(
                        sortBy,
                        sortDir == "desc" ? SortDirection.Desc : SortDirection.Asc),
                search);

            var result = await _dispatcher
                .SendAsync<GetUsersQuery, PagedResult<UserDto>>(query, ct);

            return Ok(result);
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <remarks>
        /// Creates a new user with the provided details. Returns the ID of the created user. Requires Admin role.
        /// </remarks>
        [HttpPost]
        [Authorize(Policy = "RequireAdminRole")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserRequest request,
            CancellationToken ct)
        {
            var command = new CreateUserCommand(
                request.Email,
                request.FirstName,
                request.LastName,
                request.Role);

            var userId = await _dispatcher
                .SendAsync<CreateUserCommand, int>(command, ct);

            return CreatedAtAction(
                nameof(GetUser),
                new { id = userId },
                userId);
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <remarks>
        /// Updates an existing user's details. Non-admin users can only update their own FirstName, LastName, and Password. 
        /// Admins can update any user's Email and Role.
        /// </remarks>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(
            [FromRoute] int id,
            [FromBody] UpdateUserRequest request,
            CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out var currentUserId) || string.IsNullOrEmpty(userRoleClaim))
            {
                return Unauthorized();
            }

            var command = new UpdateUserCommand(
                id,
                request.Email,
                request.FirstName,
                request.LastName,
                request.Role,
                request.NewPassword,
                currentUserId,
                userRoleClaim);

            try
            {
                var result = await _dispatcher
                    .SendAsync<UpdateUserCommand, bool>(command, ct);

                return result ? NoContent() : NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <remarks>
        /// Deletes a user by their unique identifier. Returns 204 No Content on success or 404 Not Found if the user 
        /// does not exist. Requires Admin role.
        /// </remarks>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "RequireAdminRole")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var command = new RemoveUserCommand(id);

            var result = await _dispatcher
                .SendAsync<RemoveUserCommand, bool>(command, ct);

            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Get users for dropdown selection
        /// </summary>
        /// <remarks>
        /// Retrieves a simplified list of active users for dropdown/selection purposes. Returns user ID and full name only.
        /// </remarks>
        [HttpGet("lookup")]
        [ProducesResponseType(typeof(List<UserLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsersLookup(CancellationToken ct)
        {
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Select(u => new UserLookupDto(
                    u.UserId,
                    $"{u.FirstName} {u.LastName}"))
                .ToListAsync(ct);

            return Ok(users);
        }
    }

}
