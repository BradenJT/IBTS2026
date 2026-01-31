using FluentValidation;

namespace IBTS2026.Application.Features.Auth.InviteUser;

public sealed class InviteUserCommandValidator : AbstractValidator<InviteUserCommand>
{
    private static readonly string[] ValidRoles = ["Admin", "User"];

    public InviteUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(250).WithMessage("Email must not exceed 250 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => ValidRoles.Contains(role)).WithMessage("Role must be either 'Admin' or 'User'.");

        RuleFor(x => x.InvitedByUserId)
            .GreaterThan(0).WithMessage("InvitedByUserId must be a valid user ID.");
    }
}
