using System.Security.Claims;
using IBTS2026.Api.Contracts.Auth;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Application.Features.Auth.InviteUser;
using IBTS2026.Application.Features.Auth.Login;
using IBTS2026.Application.Features.Auth.RegisterUser;
using IBTS2026.Domain.Interfaces.Users;

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
                request.LastName,
                request.InvitationToken);

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
        .WithDescription("Creates a new user account. The first registered user automatically becomes an Admin. Subsequent registrations require a valid invitation token.")
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

        app.MapPost("/auth/invite", async (
            InviteRequest request,
            HttpContext httpContext,
            IRequestDispatcher dispatcher,
            CancellationToken ct) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var invitedByUserId))
            {
                return Results.Unauthorized();
            }

            var command = new InviteUserCommand(
                request.Email,
                request.Role,
                invitedByUserId);

            var result = await dispatcher
                .SendAsync<InviteUserCommand, InviteUserResult>(command, ct);

            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.ErrorMessage });
            }

            return Results.Created(
                $"/auth/invitations/{result.InvitationId}",
                new InviteResponse(
                    result.InvitationId!.Value,
                    result.Email!,
                    request.Role,
                    result.ExpiresAt!.Value));
        })
        .RequireAuthorization("RequireAdminRole")
        .WithName("InviteUser")
        .WithSummary("Invite a new user")
        .WithDescription("Creates an invitation for a new user and sends them an email with a registration link. Requires Admin role.")
        .WithTags("Auth")
        .Produces<InviteResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        app.MapGet("/auth/check-first-user", async (
            IUserRepository userRepository,
            CancellationToken ct) =>
        {
            var anyUsersExist = await userRepository.AnyUsersExistAsync(ct);
            return Results.Ok(new { IsFirstUser = !anyUsersExist });
        })
        .AllowAnonymous()
        .WithName("CheckFirstUser")
        .WithSummary("Check if this is the first user registration")
        .WithDescription("Returns whether this would be the first user registration (which would grant Admin role).")
        .WithTags("Auth")
        .Produces<object>(StatusCodes.Status200OK);

        app.MapGet("/auth/validate-invitation", async (
            string token,
            IUserInvitationRepository invitationRepository,
            CancellationToken ct) =>
        {
            var invitation = await invitationRepository.GetByTokenAsync(token, ct);

            if (invitation is null || !invitation.IsValid)
            {
                return Results.NotFound(new { error = "Invalid or expired invitation token." });
            }

            return Results.Ok(new
            {
                Email = invitation.Email,
                Role = invitation.Role,
                ExpiresAt = invitation.ExpiresAt
            });
        })
        .AllowAnonymous()
        .WithName("ValidateInvitation")
        .WithSummary("Validate an invitation token")
        .WithDescription("Validates an invitation token and returns the invitation details if valid.")
        .WithTags("Auth")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // Invitation Management Endpoints (Admin Only)
        app.MapGet("/auth/invitations", async (
            IUserInvitationRepository invitationRepository,
            IUserRepository userRepository,
            CancellationToken ct) =>
        {
            var invitations = await invitationRepository.GetAllInvitationsAsync(ct);

            var result = new List<InvitationListItem>();
            foreach (var inv in invitations)
            {
                var invitedBy = await userRepository.GetByIdAsync(inv.InvitedByUserId, ct);
                result.Add(new InvitationListItem(
                    inv.Id,
                    inv.Email,
                    inv.Role,
                    inv.CreatedAt,
                    inv.ExpiresAt,
                    inv.IsUsed,
                    inv.UsedAt,
                    inv.IsValid,
                    invitedBy?.FirstName + " " + invitedBy?.LastName));
            }

            return Results.Ok(result);
        })
        .RequireAuthorization("RequireAdminRole")
        .WithName("GetInvitations")
        .WithSummary("Get all invitations")
        .WithDescription("Returns a list of all invitations (pending, used, and expired). Requires Admin role.")
        .WithTags("Auth")
        .Produces<List<InvitationListItem>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden);

        app.MapDelete("/auth/invitations/{id:int}", async (
            int id,
            IUserInvitationRepository invitationRepository,
            IUnitOfWork unitOfWork,
            CancellationToken ct) =>
        {
            var invitation = await invitationRepository.GetByIdAsync(id, ct);

            if (invitation is null)
            {
                return Results.NotFound(new { error = "Invitation not found." });
            }

            if (invitation.IsUsed)
            {
                return Results.BadRequest(new { error = "Cannot cancel an invitation that has already been used." });
            }

            invitationRepository.Remove(invitation);
            await unitOfWork.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .RequireAuthorization("RequireAdminRole")
        .WithName("CancelInvitation")
        .WithSummary("Cancel an invitation")
        .WithDescription("Cancels (deletes) a pending invitation. Cannot cancel used invitations. Requires Admin role.")
        .WithTags("Auth")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/auth/invitations/{id:int}/resend", async (
            int id,
            IUserInvitationRepository invitationRepository,
            IUserRepository userRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            CancellationToken ct) =>
        {
            var invitation = await invitationRepository.GetByIdAsync(id, ct);

            if (invitation is null)
            {
                return Results.NotFound(new { error = "Invitation not found." });
            }

            if (invitation.IsUsed)
            {
                return Results.BadRequest(new { error = "Cannot resend an invitation that has already been used." });
            }

            // If expired, extend the expiration
            if (invitation.ExpiresAt <= DateTime.UtcNow)
            {
                invitation.GetType().GetProperty("ExpiresAt")!.SetValue(invitation, DateTime.UtcNow.AddDays(7));
                invitationRepository.Update(invitation);
                await unitOfWork.SaveChangesAsync(ct);
            }

            var invitedBy = await userRepository.GetByIdAsync(invitation.InvitedByUserId, ct);
            if (invitedBy is null)
            {
                return Results.BadRequest(new { error = "Original inviter not found." });
            }

            var emailSent = await emailService.SendInvitationEmailAsync(
                invitation.Email,
                invitedBy.FirstName,
                invitedBy.LastName,
                invitation.Role,
                invitation.Token,
                invitation.ExpiresAt,
                ct);

            if (!emailSent)
            {
                return Results.StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Results.Ok(new { message = "Invitation email resent successfully." });
        })
        .RequireAuthorization("RequireAdminRole")
        .WithName("ResendInvitation")
        .WithSummary("Resend an invitation email")
        .WithDescription("Resends the invitation email. If expired, extends the expiration by 7 days. Requires Admin role.")
        .WithTags("Auth")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public record InvitationListItem(
    int Id,
    string Email,
    string Role,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsUsed,
    DateTime? UsedAt,
    bool IsValid,
    string InvitedByName);
