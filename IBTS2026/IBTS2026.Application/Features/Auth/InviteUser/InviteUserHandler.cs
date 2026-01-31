using FluentValidation;
using IBTS2026.Application.Abstractions.Persistence;
using IBTS2026.Application.Abstractions.Requests;
using IBTS2026.Application.Abstractions.Services;
using IBTS2026.Domain.Entities.Features.Users;
using IBTS2026.Domain.Interfaces.Users;

namespace IBTS2026.Application.Features.Auth.InviteUser;

public sealed class InviteUserHandler(
    IUserRepository users,
    IUserInvitationRepository invitations,
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    IValidator<InviteUserCommand> validator) : IRequestHandler<InviteUserCommand, InviteUserResult>
{
    private readonly IUserRepository _users = users ?? throw new ArgumentNullException(nameof(users));
    private readonly IUserInvitationRepository _invitations = invitations ?? throw new ArgumentNullException(nameof(invitations));
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IEmailService _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    private readonly IValidator<InviteUserCommand> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<InviteUserResult> Handle(InviteUserCommand command, CancellationToken ct)
    {
        var validationResult = await _validator.ValidateAsync(command, ct);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return new InviteUserResult(false, null, null, null, null, errors);
        }

        // Check if the inviting user exists and is an admin
        var invitingUser = await _users.GetByIdAsync(command.InvitedByUserId, ct);
        if (invitingUser is null)
        {
            return new InviteUserResult(false, null, null, null, null, "Inviting user not found.");
        }

        if (!string.Equals(invitingUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return new InviteUserResult(false, null, null, null, null, "Only administrators can invite new users.");
        }

        // Check if a user with this email already exists
        var existingUser = await _users.GetByEmailAsync(command.Email, ct);
        if (existingUser is not null)
        {
            return new InviteUserResult(false, null, null, null, null, "A user with this email address already exists.");
        }

        // Check if there's already a pending invitation for this email
        var existingInvitation = await _invitations.GetByEmailAsync(command.Email, ct);
        if (existingInvitation is not null)
        {
            return new InviteUserResult(false, null, null, null, null,
                "An active invitation already exists for this email address. You can resend or cancel it.");
        }

        // Create the invitation
        var invitation = UserInvitation.Create(
            command.Email,
            command.Role,
            command.InvitedByUserId,
            expirationDays: 7);

        _invitations.Add(invitation);
        await _unitOfWork.SaveChangesAsync(ct);

        // Send invitation email (using the IEmailService which knows the base URL)
        await _emailService.SendInvitationEmailAsync(
            command.Email,
            invitingUser.FirstName,
            invitingUser.LastName,
            command.Role,
            invitation.Token,
            invitation.ExpiresAt,
            ct);

        return new InviteUserResult(
            true,
            invitation.Id,
            invitation.Email,
            invitation.Token,
            invitation.ExpiresAt,
            null);
    }
}
