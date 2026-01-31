using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Domain.Entities.Features.Users;
using IBTS2026.Domain.Interfaces.Users;

namespace IBTS2026.Application.Features.Auth.RegisterUser;

public sealed class RegisterUserHandler(
    IUserRepository users,
    IUserInvitationRepository invitations,
    IUnitOfWork unitOfWork,
    IPasswordHashingService passwordHasher,
    IValidator<RegisterUserCommand> validator) : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _users = users ?? throw new ArgumentNullException(nameof(users));
    private readonly IUserInvitationRepository _invitations = invitations ?? throw new ArgumentNullException(nameof(invitations));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IPasswordHashingService _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    private readonly IValidator<RegisterUserCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<RegisterUserResult> Handle(RegisterUserCommand command, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return new RegisterUserResult(false, null, null, null, errors);
        }

        // Check if any users exist (first user scenario)
        var anyUsersExist = await _users.AnyUsersExistAsync(ct);

        // If users exist, registration requires a valid invitation token
        UserInvitation? invitation = null;
        string role;

        if (anyUsersExist)
        {
            // Invitation token is required for all registrations after the first user
            if (string.IsNullOrWhiteSpace(command.InvitationToken))
            {
                return new RegisterUserResult(false, null, null, null,
                    "Registration is invite-only. Please use a valid invitation link to register.");
            }

            // Validate the invitation token
            invitation = await _invitations.GetByTokenAsync(command.InvitationToken, ct);
            if (invitation is null)
            {
                return new RegisterUserResult(false, null, null, null,
                    "Invalid invitation token. Please request a new invitation from an administrator.");
            }

            if (!invitation.IsValid)
            {
                return new RegisterUserResult(false, null, null, null,
                    "This invitation has expired or has already been used. Please request a new invitation.");
            }

            // Verify the email matches the invitation
            if (!string.Equals(invitation.Email, command.Email, StringComparison.OrdinalIgnoreCase))
            {
                return new RegisterUserResult(false, null, null, null,
                    "The email address does not match the invitation. Please use the email address the invitation was sent to.");
            }

            role = invitation.Role;
        }
        else
        {
            // First user becomes Admin automatically
            role = "Admin";
        }

        // Check if email is already taken
        var existingUser = await _users.GetByEmailAsync(command.Email, ct);
        if (existingUser is not null)
        {
            return new RegisterUserResult(false, null, null, null, "Email is already registered.");
        }

        // Hash password and create user
        var passwordHash = _passwordHasher.HashPassword(command.Password);
        var user = User.CreateWithPassword(
            command.Email,
            command.FirstName,
            command.LastName,
            role,
            passwordHash);

        _users.Add(user);

        // Mark invitation as used if present
        if (invitation is not null)
        {
            invitation.MarkAsUsed();
            _invitations.Update(invitation);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return new RegisterUserResult(true, user.UserId, user.Email, user.Role, null);
    }
}
