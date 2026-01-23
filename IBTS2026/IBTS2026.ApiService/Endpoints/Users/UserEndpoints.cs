using IBTS2026.Api.Contracts.Users;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Dtos.Users;
using IBTS2026.Application.Features.Users.CreateUser;
using IBTS2026.Application.Features.Users.GetUser;
using IBTS2026.Application.Features.Users.GetUsers;
using IBTS2026.Application.Features.Users.RemoveUser;
using IBTS2026.Application.Features.Users.UpdateUser;
using IBTS2026.Application.Models.Requests;

namespace IBTS2026.ApiService.Endpoints.Users
{
    public static class UserEndpoints
    {
        public static void Map(WebApplication app)
        { 
            app.MapGet("/users/{id:int}", async (
                int id,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var result = await dispatcher
                .SendAsync<GetUserQuery, UserDetailsDto?>(
                    new GetUserQuery(id), ct);

                return result is null
                ? Results.NotFound()
                : Results.Ok(result);
            });

            app.MapGet("/users", async (
                int pageNumber,
                int pageSize,
                string? search,
                string? sortBy,
                string? sortDir,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var query = new GetUsersQuery(
                    new PageRequest(pageNumber, pageSize),
                    sortBy is null
                    ? null
                    : new SortRequest(
                        sortBy,
                        sortDir == "desc" ? SortDirection.Desc : SortDirection.Asc),
                    search
            );
                var result = await dispatcher
                .SendAsync<GetUsersQuery, PagedResult<UserDto>>(query, ct);

                return Results.Ok(result);
            });

            app.MapPost("/users", async (
                CreateUserRequest request,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var command = new CreateUserCommand(
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.Role
                    );

                var userId = await dispatcher
                .SendAsync<CreateUserCommand, int>(command, ct);

                return Results.Created($"/users/{userId}", userId);
            })
            .WithName("CreateUser")
            .WithSummary("Create a new user")
            .WithDescription("Creates a new user with the provided details. Returns the ID of the created user.")
            .WithTags("Users")
            .Produces<int>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapPut("/users/{id:int}", async (
                int id,
                UpdateUserRequest request,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var command = new UpdateUserCommand(
                    id,
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.Role
                    );

                var result = await dispatcher
                .SendAsync<UpdateUserCommand, bool>(command, ct);

                return result ? Results.NoContent() : Results.NotFound();
            });

            app.MapDelete("/users/{id:int}", async (
                int id,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var command = new RemoveUserCommand(id);

                var result = await dispatcher
                .SendAsync<RemoveUserCommand, bool>(command, ct);

                return result ? Results.NoContent() : Results.NotFound();
            });
        }
    }
}
