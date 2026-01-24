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
            })
            .WithName("GetUser")
            .WithSummary("Get a user by ID")
            .WithDescription("Retrieves detailed information about a specific user by their unique identifier.")
            .WithTags("Users")
            .Produces<UserDetailsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapGet("/users", async (
                int? pageNumber,
                int? pageSize,
                string? search,
                string? sortBy,
                string? sortDir,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var query = new GetUsersQuery(
                    new PageRequest(pageNumber ?? 1, pageSize ?? 20),
                    sortBy is null
                        ? null
                        : new SortRequest(
                            sortBy,
                            sortDir == "desc" ? SortDirection.Desc : SortDirection.Asc),
                    search);

                var result = await dispatcher
                    .SendAsync<GetUsersQuery, PagedResult<UserDto>>(query, ct);

                return Results.Ok(result);
            })
            .WithName("GetUsers")
            .WithSummary("Get a paginated list of users")
            .WithDescription("Retrieves a paginated list of users with optional search and sorting. Defaults to page 1 with 20 items per page.")
            .WithTags("Users")
            .Produces<PagedResult<UserDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapPost("/users", async (
                CreateUserRequest request,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var command = new CreateUserCommand(
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.Role);

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
                    request.Role);

                var result = await dispatcher
                    .SendAsync<UpdateUserCommand, bool>(command, ct);

                return result ? Results.NoContent() : Results.NotFound();
            })
            .WithName("UpdateUser")
            .WithSummary("Update an existing user")
            .WithDescription("Updates an existing user's details by their unique identifier. Returns 204 No Content on success or 404 Not Found if the user does not exist.")
            .WithTags("Users")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapDelete("/users/{id:int}", async (
                int id,
                IRequestDispatcher dispatcher,
                CancellationToken ct) =>
            {
                var command = new RemoveUserCommand(id);

                var result = await dispatcher
                    .SendAsync<RemoveUserCommand, bool>(command, ct);

                return result ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteUser")
            .WithSummary("Delete a user")
            .WithDescription("Deletes a user by their unique identifier. Returns 204 No Content on success or 404 Not Found if the user does not exist.")
            .WithTags("Users")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}
