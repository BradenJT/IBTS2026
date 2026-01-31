using IBTS2026.Api.Contracts.Auth;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Application.Features.Auth.InviteUser;
using IBTS2026.Application.Features.Auth.Login;
using IBTS2026.Application.Features.Auth.RegisterUser;
using IBTS2026.Domain.Interfaces.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IBTS2026.ApiService.Controllers.Auth
{
    [ApiController]
    [Route("auth")]
    [Produces("application/json")]
    public class AuthController(
        IRequestDispatcher dispatcher,
        IUserRepository userRepository,
        IUserInvitationRepository invitationRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork) : ControllerBase
    {
        private readonly IRequestDispatcher _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IUserInvitationRepository _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
        private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));


        /// <summary>
        /// Register a new user
        /// </summary>
        /// <remarks>
        /// Creates a new user account. The first registered user automatically becomes an Admin. 
        /// Subsequent registrations require a valid invitation token.
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.InvitationToken);

            var result = await _dispatcher
                .SendAsync<RegisterUserCommand, RegisterUserResult>(command, ct);

            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            return CreatedAtAction(
                nameof(Register),
                new { userId = result.UserId!.Value },
                new RegisterResponse(result.UserId!.Value, result.Email!, result.Role!));
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <remarks>
        /// Authenticates a user and returns a JWT token for subsequent API calls.
        /// </remarks>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken ct)
        {
            var command = new LoginCommand(request.Email, request.Password);

            var result = await _dispatcher
                .SendAsync<LoginCommand, LoginResult>(command, ct);

            if (!result.Success)
            {
                return Unauthorized();
            }

            return Ok(new LoginResponse(
                result.UserId!.Value,
                result.Email!,
                result.FirstName!,
                result.LastName!,
                result.Role!,
                result.Token!,
                result.SecurityStamp!));
        }

        /// <summary>
        /// Invite a new user
        /// </summary>
        /// <remarks>
        /// Creates an invitation for a new user and sends them an email with a registration link. 
        /// Requires Admin role.
        /// </remarks>
        [HttpPost("invite")]
        [Authorize(Policy = "RequireAdminRole")]
        [ProducesResponseType(typeof(InviteResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InviteUser(
            [FromBody] InviteRequest request,
            CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var invitedByUserId))
            {
                return Unauthorized();
            }

            var command = new InviteUserCommand(
                request.Email,
                request.Role,
                invitedByUserId);

            var result = await _dispatcher
                .SendAsync<InviteUserCommand, InviteUserResult>(command, ct);

            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }

            return CreatedAtAction(
                nameof(ValidateInvitation),
                new { token = result.InvitationId },
                new InviteResponse(
                    result.InvitationId!.Value,
                    result.Email!,
                    request.Role,
                    result.ExpiresAt!.Value));
        }

        /// <summary>
        /// Check if this is the first user registration
        /// </summary>
        /// <remarks>
        /// Returns whether this would be the first user registration (which would grant Admin role).
        /// </remarks>
        [HttpGet("check-first-user")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckFirstUser(CancellationToken ct)
        {
            var anyUsersExist = await _userRepository.AnyUsersExistAsync(ct);
            return Ok(new { IsFirstUser = !anyUsersExist });
        }

        /// <summary>
        /// Validate a user's security stamp
        /// </summary>
        /// <remarks>
        /// Validates that the user's security stamp matches the one in the database.
        /// Used to check if a session is still valid after security-sensitive changes.
        /// </remarks>
        [HttpPost("validate-security-stamp")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ValidateSecurityStampResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateSecurityStamp(
            [FromBody] ValidateSecurityStampRequest request,
            CancellationToken ct)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, ct);

            if (user is null)
            {
                return Ok(new ValidateSecurityStampResponse(false));
            }

            var isValid = user.SecurityStamp == request.SecurityStamp && user.IsActive;
            return Ok(new ValidateSecurityStampResponse(isValid));
        }

        /// <summary>
        /// Validate an invitation token
        /// </summary>
        /// <remarks>
        /// Validates an invitation token and returns the invitation details if valid.
        /// </remarks>
        [HttpGet("validate-invitation")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ValidateInvitation(
            [FromQuery] string token,
            CancellationToken ct)
        {
            var invitation = await _invitationRepository.GetByTokenAsync(token, ct);

            if (invitation is null || !invitation.IsValid)
            {
                return NotFound(new { error = "Invalid or expired invitation token." });
            }

            return Ok(new
            {
                Email = invitation.Email,
                Role = invitation.Role,
                ExpiresAt = invitation.ExpiresAt
            });
        }

        /// <summary>
        /// Get all invitations
        /// </summary>
        /// <remarks>
        /// Returns a list of all invitations (pending, used, and expired). Requires Admin role.
        /// </remarks>
        [HttpGet("invitations")]
        [Authorize(Policy = "RequireAdminRole")]
        [ProducesResponseType(typeof(List<InvitationListItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetInvitations(CancellationToken ct)
        {
            var invitations = await _invitationRepository.GetAllInvitationsAsync(ct);

            var result = new List<InvitationListItem>();
            foreach (var inv in invitations)
            {
                var invitedBy = await _userRepository.GetByIdAsync(inv.InvitedByUserId, ct);
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

            return Ok(result);
        }

        /// <summary>
        /// Cancel an invitation
        /// </summary>
        /// <remarks>
        /// Cancels (deletes) a pending invitation. Cannot cancel used invitations. Requires Admin role.
        /// </remarks>
        [HttpDelete("invitations/{id:int}")]
        [Authorize(Policy = "RequireAdminRole")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelInvitation(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var invitation = await _invitationRepository.GetByIdAsync(id, ct);

            if (invitation is null)
            {
                return NotFound(new { error = "Invitation not found." });
            }

            if (invitation.IsUsed)
            {
                return BadRequest(new { error = "Cannot cancel an invitation that has already been used." });
            }

            _invitationRepository.Remove(invitation);
            await _unitOfWork.SaveChangesAsync(ct);

            return NoContent();
        }

        /// <summary>
        /// Resend an invitation email
        /// </summary>
        /// <remarks>
        /// Resends the invitation email. If expired, extends the expiration by 7 days. Requires Admin role.
        /// </remarks>
        [HttpPost("invitations/{id:int}/resend")]
        [Authorize(Policy = "RequireAdminRole")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResendInvitation(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var invitation = await _invitationRepository.GetByIdAsync(id, ct);

            if (invitation is null)
            {
                return NotFound(new { error = "Invitation not found." });
            }

            if (invitation.IsUsed)
            {
                return BadRequest(new { error = "Cannot resend an invitation that has already been used." });
            }

            // If expired, extend the expiration
            if (invitation.ExpiresAt <= DateTime.UtcNow)
            {
                invitation.GetType().GetProperty("ExpiresAt")!.SetValue(invitation, DateTime.UtcNow.AddDays(7));
                _invitationRepository.Update(invitation);
                await _unitOfWork.SaveChangesAsync(ct);
            }

            var invitedBy = await _userRepository.GetByIdAsync(invitation.InvitedByUserId, ct);
            if (invitedBy is null)
            {
                return BadRequest(new { error = "Original inviter not found." });
            }

            var emailSent = await _emailService.SendInvitationEmailAsync(
                invitation.Email,
                invitedBy.FirstName,
                invitedBy.LastName,
                invitation.Role,
                invitation.Token,
                invitation.ExpiresAt,
                ct);

            if (!emailSent)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok(new { message = "Invitation email resent successfully." });
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
}
