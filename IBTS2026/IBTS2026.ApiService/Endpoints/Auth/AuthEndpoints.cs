using IBTS2026.Api.Contracts.Auth;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Features.Auth.Login;
using IBTS2026.Application.Features.Auth.RegisterUser;

namespace IBTS2026.ApiService.Endpoints.Auth;

public static class AuthEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/auth/register", async (
            RegisterRequest request,
            IRequestDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName);

            var result = await dispatcher
                .SendAsync<RegisterUserCommand, RegisterUserResult>(command, ct);

            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.ErrorMessage });
            }

            return Results.Created(
                $"/users/{result.UserId}",
                new RegisterResponse(result.UserId!.Value, result.Email!, result.Role!));
        })
        .AllowAnonymous()
        .WithName("Register")
        .WithSummary("Register a new user")
        .WithDescription("Creates a new user account. The first registered user automatically becomes an Admin.")
        .WithTags("Auth")
        .Produces<RegisterResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        app.MapPost("/auth/login", async (
            LoginRequest request,
            IRequestDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var command = new LoginCommand(request.Email, request.Password);

            var result = await dispatcher
                .SendAsync<LoginCommand, LoginResult>(command, ct);

            if (!result.Success)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new LoginResponse(
                result.UserId!.Value,
                result.Email!,
                result.FirstName!,
                result.LastName!,
                result.Role!,
                result.Token!));
        })
        .AllowAnonymous()
        .WithName("Login")
        .WithSummary("Login with email and password")
        .WithDescription("Authenticates a user and returns a JWT token for subsequent API calls.")
        .WithTags("Auth")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
